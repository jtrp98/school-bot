using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

public sealed class LearningSubmissionTests
{
    private static AssessmentPrincipalContext Student => new(
        CurriculumPrincipalStub.CreateDesktopStudent("301") with { SchoolId = 11 }, null, "student", 301);

    private static async Task<int> Project(CourseRunFixture fixture, string mode, string content, string kind = "student") =>
        (await fixture.Reader().CommitProjectAsync(new(null, 11, kind, 301, mode, "Homework", content)))
            .Revision.SchoolBotProjectRevisionId;

    private static LearningSubmissionService Service(CourseRunFixture fixture, IAssessmentAuthority? authority = null) =>
        new(fixture.Reader(), authority ?? fixture, fixture.Reader());

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SubmissionSurvivesRestartRegradesServerSourceAndDeduplicates(bool file)
    {
        using var fixture = new CourseRunFixture(file);
        await fixture.Initialize();
        var revision = await Project(fixture, "code", """{"kind":"code","source":"hero.jump()"}""");
        var key = Guid.NewGuid();
        var request = new CreateSubmissionRequest(3, 3, revision);
        var accepted = await Service(fixture).CreateAsync(Student, request, key);
        Assert.Equal("queued", accepted.Submission.State);
        Assert.Null(accepted.Assessment);
        Assert.Single((await fixture.Snapshot()).GradingJobs!);
        var replay = await Service(fixture).CreateAsync(Student, request, key);
        Assert.Equal(accepted.Submission.Id, replay.Submission.Id);
        var graded = await Service(fixture).GetAsync(Student, accepted.Submission.Id);
        Assert.Equal("passed", graded.Assessment!.Outcome);
        Assert.Null(graded.Assessment.EarnedScore);
        Assert.Equal(LearningProgramEvaluator.Version, graded.Assessment.GraderVersion);
        await Service(fixture).GetAsync(Student, accepted.Submission.Id);
        Assert.Single((await fixture.Snapshot()).Assessments!);
        var repository = fixture.Reader();
        var progress = await new AssessmentReadService(repository, fixture, fixture.Clock, repository).GetProgressAsync(Student, 1);
        Assert.NotNull(progress.Activities.Single(a => a.Mode == "code").LatestSubmission);
    }

    [Fact]
    public async Task WorkerGradesQueuedImmutableRevisionWithoutAnInteractiveStudentRequest()
    {
        using var fixture = new CourseRunFixture(true);
        await fixture.Initialize();
        var revision = await Project(fixture, "code", """{"kind":"code","source":"hero.jump()"}""");
        var accepted = await Service(fixture).CreateAsync(Student, new(3, 3, revision), Guid.NewGuid());
        Assert.Equal(1, await Service(fixture).SweepAsync());
        Assert.Equal(0, await Service(fixture).SweepAsync());
        Assert.Equal("evaluated", (await fixture.Reader().GetSubmissionAsync(accepted.Submission.Id))!.State);
        Assert.Single((await fixture.Snapshot()).Assessments!);
    }

    [Fact]
    public async Task EmployeeOwnedProjectAndInvalidSourceCannotCreateCloudResults()
    {
        using var fixture = new CourseRunFixture(false);
        await fixture.Initialize();
        var revision = await Project(fixture, "code", """{"kind":"code","source":"hero.jump()"}""", "employee");
        var rejected = await Assert.ThrowsAsync<DomainRuleException>(() => Service(fixture).CreateAsync(Student,
            new(3, 3, revision), Guid.NewGuid()));
        Assert.Equal(ErrorCodes.NotFound, rejected.Code);
        var invalid = await Project(fixture, "code", """{"kind":"code","source":"import os"}""");
        Assert.Equal("PROGRAM_INVALID", (await Assert.ThrowsAsync<DomainRuleException>(() => Service(fixture)
            .CreateAsync(Student, new(3, 3, invalid), Guid.NewGuid()))).Code);
        Assert.Empty((await fixture.Snapshot()).Submissions ?? []);
    }

    [Fact]
    public async Task ArmReviewRequiresAssignedTeacherAndPreservesCorrectionHistory()
    {
        using var fixture = new CourseRunFixture(false);
        await fixture.Initialize();
        var revision = await Project(fixture, "arm", "{}");
        var accepted = await Service(fixture).CreateAsync(Student, new(2, 2, revision), Guid.NewGuid());
        Assert.Equal("awaiting_teacher", accepted.Submission.State);
        var teacher = new AssessmentPrincipalContext(Student.Session with { Channel = "school_web", PrincipalId = "teacher:77" },
            77, "employee");
        var review = new ReviewSubmissionRequest("passed", "Checked against rubric", [new(CourseRunSeed.Key(701), true)]);
        var denied = await Assert.ThrowsAsync<DomainRuleException>(() => Service(fixture).ReviewAsync(teacher,
            accepted.Submission.Id, review, OpaqueConcurrencyToken.ToETag(accepted.Submission.UpdateDate), Guid.NewGuid()));
        Assert.Equal(ErrorCodes.ProfileNotAllowed, denied.Code);
        var service = Service(fixture, new TeacherAuthority(fixture));
        var assessed = await service.ReviewAsync(teacher, accepted.Submission.Id, review,
            OpaqueConcurrencyToken.ToETag(accepted.Submission.UpdateDate), Guid.NewGuid());
        Assert.Equal(77, assessed.Assessment!.AssessorId);
        var conflict = await Assert.ThrowsAsync<DomainRuleException>(() => service.ReviewAsync(teacher,
            accepted.Submission.Id, review, OpaqueConcurrencyToken.ToETag(accepted.Submission.UpdateDate), Guid.NewGuid()));
        Assert.Equal(ErrorCodes.VersionConflict, conflict.Code);
        var correction = review with { Outcome = "not_yet", SupersedesId = assessed.Assessment.Id, Reason = "Rubric correction" };
        var corrected = await service.ReviewAsync(teacher, accepted.Submission.Id, correction,
            OpaqueConcurrencyToken.ToETag(assessed.Submission.UpdateDate), Guid.NewGuid());
        Assert.Equal(assessed.Assessment.Id, corrected.Assessment!.SupersedesId);
        Assert.Equal(2, (await fixture.Snapshot()).Assessments!.Count);
    }

    [Theory]
    [InlineData("hero.jump()", "passed")]
    [InlineData("hero.move_right(2)", "not_yet")]
    public void BoundedEvaluatorUsesApprovedRules(string source, string expected)
    {
        var content = System.Text.Json.JsonSerializer.Serialize(new { kind = "code", source });
        var commands = LearningProgramEvaluator.Compile("code", content);
        var criteria = """{"kind":"automatic-v1","combine":"all","rules":[{"id":"00000000-0000-4000-8000-000000000001","type":"uses_op","op":"jump","minimum":1}]}""";
        Assert.Equal(expected, LearningProgramEvaluator.Evaluate(commands, criteria).Outcome);
        Assert.Throws<DomainRuleException>(() => LearningProgramEvaluator.Compile("code",
            """{"kind":"code","source":"hero.move_right(99)"}"""));
    }

    private sealed class TeacherAuthority(CourseRunFixture fixture) : IAssessmentAuthority
    {
        public Task<bool> CanReadRunAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run, CancellationToken ct = default) =>
            fixture.CanReadRunAsync(principal, run, ct);
        public Task<bool> CanReviewLearnerAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run, int learnerId,
            CancellationToken ct = default) => Task.FromResult(principal.EmployeeId == 77 && run.SchoolId == 11 && learnerId == 301);
    }
}

