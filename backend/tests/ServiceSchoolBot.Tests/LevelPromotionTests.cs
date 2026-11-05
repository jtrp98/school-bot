using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

public sealed class LevelPromotionTests
{
    private static AssessmentPrincipalContext Student => new(
        CurriculumPrincipalStub.CreateDesktopStudent("301") with { SchoolId = 11 }, null, "student", 301);
    private static AssessmentPrincipalContext Teacher => new(Student.Session with { Channel = "school_web", PrincipalId = "teacher:77" }, 77, "employee");

    private static async Task<Authority> Prepare(CourseRunFixture fixture, decimal threshold = 70, bool maxLevel = false)
    {
        await fixture.Initialize();
        var authority = new Authority(fixture);
        await ((ICurriculumBackingStore)fixture.Store()).MutateCurriculumUnitAsync((snapshot, _) =>
            Task.FromResult(new CurriculumStoreMutation<bool>(snapshot with
            {
                CourseRuns = snapshot.CourseRuns!.Select(r => r with { CurrentPolicyId = 1 }).ToArray(),
                ScoringPolicies = [new(1, 11, 1, 1, new(false,
                    [new(4, 0), new(5, 0), new(6, 0), new(7, 0), new(8, 10000)], "weighted-normalized-v1"), 77, fixture.Clock.GetUtcNow())],
                Levels = maxLevel ? [new(1, 1, "Beginner")] : [new(1, 1, "Beginner"), new(2, 2, "Intermediate")],
                PromotionPolicies = [new(1, 1, 1, threshold)],
                Curricula = maxLevel ? snapshot.Curricula : [.. snapshot.Curricula,
                    snapshot.Curricula[0] with { SchoolBotCurriculumId = 2, LevelId = 2, CurriculumKey = Guid.NewGuid() }]
            }, true)));
        var repository = fixture.Reader();
        var exams = new AssessmentReadService(repository, authority, fixture.Clock, repository);
        var attempt = await exams.StartAttemptAsync(Student, 1, 8, Guid.NewGuid());
        await exams.SaveAttemptAsync(Student, attempt.Id,
            [new(QuestionBankSeed.Key(201), [QuestionBankSeed.Key(211)]), new(QuestionBankSeed.Key(202), [QuestionBankSeed.Key(221)])],
            true, OpaqueConcurrencyToken.ToETag(attempt.UpdateDate), Guid.NewGuid());
        await ((ICurriculumBackingStore)fixture.Store()).MutateCurriculumUnitAsync((snapshot, _) =>
            Task.FromResult(new CurriculumStoreMutation<bool>(snapshot with
            { CourseRuns = snapshot.CourseRuns!.Select(r => r with { Lifecycle = "closed" }).ToArray() }, true)));
        return authority;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConfirmPromotesWholeClassOnceAndReplaysSuccessor(bool file)
    {
        using var fixture = new CourseRunFixture(file);
        var authority = await Prepare(fixture);
        var repository = fixture.Reader();
        var service = new LevelPromotionService(repository, repository, authority, fixture.Clock);
        var proposal = await service.CreateAsync(Teacher, 1, Guid.NewGuid());
        Assert.Equal("proposed", proposal.State);
        var key = Guid.NewGuid();
        var confirmed = await service.DecideAsync(Teacher, proposal.Id, true, proposal.SnapshotHash,
            OpaqueConcurrencyToken.ToETag(proposal.UpdateDate), null, key);
        Assert.Equal("promoted", confirmed.State);
        var replay = await service.DecideAsync(Teacher, proposal.Id, true, proposal.SnapshotHash,
            OpaqueConcurrencyToken.ToETag(proposal.UpdateDate), null, key);
        Assert.Equal(confirmed.SuccessorRunId, replay.SuccessorRunId);
        var successor = (await repository.GetCourseRunByIdAsync(confirmed.SuccessorRunId!.Value))!;
        Assert.Equal(2, successor.CurriculumId);
        Assert.Equal(401, successor.ClassroomId);
        Assert.Equal(1, successor.PreviousRunId);
        Assert.Null(successor.CurrentPolicyId);
        Assert.Equal(2, (await fixture.Snapshot()).CourseRuns!.Count);
    }

    [Theory]
    [InlineData(100, false, "retained")]
    [InlineData(70, true, "max_level")]
    public async Task EqualThresholdRetainsAndMaximumLevelNeverInventsNextLevel(int threshold, bool maxLevel, string expected)
    {
        using var fixture = new CourseRunFixture(false);
        var authority = await Prepare(fixture, threshold, maxLevel);
        var repository = fixture.Reader();
        var proposal = await new LevelPromotionService(repository, repository, authority, fixture.Clock)
            .CreateAsync(Teacher, 1, Guid.NewGuid());
        Assert.Equal(expected, proposal.State);
        Assert.Equal(1, (await repository.GetCourseRunByIdAsync(proposal.SuccessorRunId!.Value))!.CurriculumId);
    }

    [Fact]
    public async Task RosterChangeInvalidatesConfirmationAndCreatesNoSuccessor()
    {
        using var fixture = new CourseRunFixture(false);
        var authority = await Prepare(fixture);
        var repository = fixture.Reader();
        var service = new LevelPromotionService(repository, repository, authority, fixture.Clock);
        var proposal = await service.CreateAsync(Teacher, 1, Guid.NewGuid());
        authority.Version = "roster-2";
        var rejected = await Assert.ThrowsAsync<DomainRuleException>(() => service.DecideAsync(Teacher,
            proposal.Id, true, proposal.SnapshotHash, OpaqueConcurrencyToken.ToETag(proposal.UpdateDate), null, Guid.NewGuid()));
        Assert.Equal("PROPOSAL_STALE", rejected.Code);
        Assert.Single((await fixture.Snapshot()).CourseRuns!);
        Assert.Equal("proposed", (await service.GetAsync(Teacher, proposal.Id)).State);
    }

    private sealed class Authority(CourseRunFixture fixture) : IAssessmentAuthority
    {
        internal string Version { get; set; } = "roster-1";
        public Task<bool> CanReadRunAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run, CancellationToken ct = default) =>
            principal.EmployeeId == 77 ? Task.FromResult(run.SchoolId == 11) : fixture.CanReadRunAsync(principal, run, ct);
        public Task<bool> CanReviewLearnerAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run, int learnerId,
            CancellationToken ct = default) => Task.FromResult(principal.EmployeeId == 77 && run.SchoolId == 11 && learnerId == 301);
        public Task<ClassroomLearningContext> GetClassroomContextAsync(AssessmentPrincipalContext principal,
            SchoolBotCourseRunRecord run, CancellationToken ct = default) => Task.FromResult(new ClassroomLearningContext(
                [301], 102, "term-2", fixture.Clock.GetUtcNow().AddDays(30), Version));
    }
}
