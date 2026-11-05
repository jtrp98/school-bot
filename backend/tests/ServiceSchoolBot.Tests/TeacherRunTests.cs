using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

public sealed class TeacherRunTests
{
    private static AssessmentPrincipalContext Student => new(
        CurriculumPrincipalStub.CreateDesktopStudent("301") with { SchoolId = 11 }, null, "student", 301);
    private static AssessmentPrincipalContext Teacher => new(Student.Session with { Channel = "school_web", PrincipalId = "teacher:77" }, 77, "employee");

    private static ScoringPolicy Policy => new(false,
        [new(4, 0), new(5, 0), new(6, 0), new(7, 0), new(8, 10000)], "weighted-normalized-v1");

    private static async Task Prepare(CourseRunFixture fixture)
    {
        await fixture.Initialize();
        await ((ICurriculumBackingStore)fixture.Store()).MutateCurriculumUnitAsync((snapshot, _) =>
            Task.FromResult(new CurriculumStoreMutation<bool>(snapshot with
            { CourseRuns = snapshot.CourseRuns!.Select(r => r with { Lifecycle = "planned" }).ToArray() }, true)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ScoringPolicyFreezesAndTotalsUseNormalizedSavedScores(bool file)
    {
        using var fixture = new CourseRunFixture(file);
        await Prepare(fixture);
        var repository = fixture.Reader();
        var authority = new Authority(fixture);
        var service = new TeacherRunService(repository, repository, authority, fixture.Clock);
        var run = (await repository.GetCourseRunByIdAsync(1))!;
        var policy = await service.SetScoringPolicyAsync(Teacher, 1, Policy,
            OpaqueConcurrencyToken.ToETag(run.UpdateDate), Guid.NewGuid());
        run = (await repository.GetCourseRunByIdAsync(1))!;
        Assert.Equal(policy.Id, run.CurrentPolicyId);
        await service.ActivateAsync(Teacher, 1, OpaqueConcurrencyToken.ToETag(run.UpdateDate), Guid.NewGuid());
        run = (await repository.GetCourseRunByIdAsync(1))!;
        var frozen = await Assert.ThrowsAsync<DomainRuleException>(() => service.SetScoringPolicyAsync(Teacher, 1,
            Policy, OpaqueConcurrencyToken.ToETag(run.UpdateDate), Guid.NewGuid()));
        Assert.Equal(ErrorCodes.StateConflict, frozen.Code);
        var missing = await service.GetTotalAsync(Teacher, 1, 301);
        Assert.Equal(0, missing.Total);
        Assert.False(missing.Complete);
        Assert.Empty(missing.Sources);
        var exams = new AssessmentReadService(repository, authority, fixture.Clock, repository);
        var attempt = await exams.StartAttemptAsync(Student, 1, 8, Guid.NewGuid());
        await exams.SaveAttemptAsync(Student, attempt.Id, [new(QuestionBankSeed.Key(201), [QuestionBankSeed.Key(211)])],
            true, OpaqueConcurrencyToken.ToETag(attempt.UpdateDate), Guid.NewGuid());
        var total = await service.GetTotalAsync(Teacher, 1, 301);
        Assert.Equal(40m, total.Total);
        Assert.True(total.Complete);
        Assert.Equal(attempt.Id, Assert.Single(total.Sources).Id);
    }

    [Fact]
    public async Task RetakeGrantAddsOneAttemptAndIsConsumedOnlyOnceOnReplay()
    {
        using var fixture = new CourseRunFixture(false);
        await Prepare(fixture);
        var repository = fixture.Reader();
        var authority = new Authority(fixture);
        var teacher = new TeacherRunService(repository, repository, authority, fixture.Clock);
        await teacher.SetScoringPolicyAsync(Teacher, 1, Policy,
            OpaqueConcurrencyToken.ToETag((await repository.GetCourseRunByIdAsync(1))!.UpdateDate), Guid.NewGuid());
        await teacher.ActivateAsync(Teacher, 1,
            OpaqueConcurrencyToken.ToETag((await repository.GetCourseRunByIdAsync(1))!.UpdateDate), Guid.NewGuid());
        var exams = new AssessmentReadService(repository, authority, fixture.Clock, repository);
        for (var i = 0; i < 2; i++)
        {
            var attempt = await exams.StartAttemptAsync(Student, 1, 8, Guid.NewGuid());
            await exams.SaveAttemptAsync(Student, attempt.Id, [], true, OpaqueConcurrencyToken.ToETag(attempt.UpdateDate), Guid.NewGuid());
        }
        await Assert.ThrowsAsync<DomainRuleException>(() => exams.StartAttemptAsync(Student, 1, 8, Guid.NewGuid()));
        var key = Guid.NewGuid();
        var grant = await teacher.GrantRetakeAsync(Teacher, 1, 8, 301, "Additional practice", key);
        Assert.Equal(grant.Id, (await teacher.GrantRetakeAsync(Teacher, 1, 8, 301, "Additional practice", key)).Id);
        var startKey = Guid.NewGuid();
        var retake = await exams.StartAttemptAsync(Student, 1, 8, startKey);
        Assert.Equal(retake.Id, (await exams.StartAttemptAsync(Student, 1, 8, startKey)).Id);
        var savedGrant = Assert.Single((await fixture.Snapshot()).RetakeGrants!);
        Assert.Equal(retake.Id, savedGrant.ConsumedAttemptId);
        await exams.SaveAttemptAsync(Student, retake.Id, [], true, OpaqueConcurrencyToken.ToETag(retake.UpdateDate), Guid.NewGuid());
        await Assert.ThrowsAsync<DomainRuleException>(() => exams.StartAttemptAsync(Student, 1, 8, Guid.NewGuid()));
    }

    [Fact]
    public async Task RemovingTeacherAssignmentAlsoPreventsRetakeReplay()
    {
        using var fixture = new CourseRunFixture(false);
        await fixture.Initialize();
        var repository = fixture.Reader();
        var authority = new Authority(fixture);
        var service = new TeacherRunService(repository, repository, authority, fixture.Clock);
        var key = Guid.NewGuid();
        await service.GrantRetakeAsync(Teacher, 1, 8, 301, "Review", key);
        authority.Allowed = false;
        Assert.Equal(ErrorCodes.ProfileNotAllowed, (await Assert.ThrowsAsync<DomainRuleException>(() =>
            service.GrantRetakeAsync(Teacher, 1, 8, 301, "Review", key))).Code);
    }

    private sealed class Authority(CourseRunFixture fixture) : IAssessmentAuthority
    {
        internal bool Allowed { get; set; } = true;
        public Task<bool> CanReadRunAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run, CancellationToken ct = default) =>
            principal.EmployeeId == 77 ? Task.FromResult(Allowed && run.SchoolId == 11) : fixture.CanReadRunAsync(principal, run, ct);
        public Task<bool> CanReviewLearnerAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run, int learnerId,
            CancellationToken ct = default) => Task.FromResult(Allowed && principal.EmployeeId == 77 && run.SchoolId == 11 && learnerId == 301);
    }
}
