
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.Tests;

public sealed class AttemptMutationTests
{
    private static readonly AssessmentAnswer[] Correct =
    [new(QuestionBankSeed.Key(201), [QuestionBankSeed.Key(211)])];

    private static async Task Prepare(CourseRunFixture fixture)
    {
        await fixture.Initialize();
        await ((ICurriculumBackingStore)fixture.Store()).MutateCurriculumUnitAsync((s, _) =>
            Task.FromResult(new CurriculumStoreMutation<bool>(s with
            {
                CourseRuns = s.CourseRuns!.Select(r => r with { CurrentPolicyId = 1 }).ToArray()
            }, true)));
    }

    private static AssessmentPrincipalContext Principal => new(
        CurriculumPrincipalStub.CreateDesktopStudent("301") with { SchoolId = 11 }, null, "student", 301);

    private static AssessmentReadService Service(CourseRunFixture fixture)
    {
        var repository = fixture.Reader();
        return new(repository, fixture, fixture.Clock, repository);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveThenSubmitAtExactDeadlineGradesOnlyPersistedAnswers(bool file)
    {
        using var fixture = new CourseRunFixture(file);
        await Prepare(fixture);
        var service = Service(fixture);
        var started = await service.StartAttemptAsync(Principal, 1, 8, Guid.NewGuid());
        var saved = await service.SaveAttemptAsync(Principal, started.Id, Correct, false,
            OpaqueConcurrencyToken.ToETag(started.UpdateDate), Guid.NewGuid());
        fixture.Clock.Advance(TimeSpan.FromSeconds(120));
        AssessmentAnswer[] late = [.. Correct, new(QuestionBankSeed.Key(202), [QuestionBankSeed.Key(221)])];
        var error = await Assert.ThrowsAsync<DomainRuleException>(() => service.SaveAttemptAsync(Principal,
            saved.Id, late, true, OpaqueConcurrencyToken.ToETag(saved.UpdateDate), Guid.NewGuid()));
        Assert.Equal(ErrorCodes.AttemptExpired, error.Code);
        var result = await Service(fixture).GetAttemptAsync(Principal, started.Id);
        Assert.Equal("expired", result.State);
        Assert.Equal("2", result.Earned);
        Assert.Null(result.SubmittedAt);
        Assert.Single(result.Answers);
        Assert.Single((await fixture.Reader().ListAuditEventsByEntityAsync("SchoolBotAttempt", started.Id)), a => a.Event == "Attempt.Expired");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StartReplayAndConcurrentSubmitPersistOneAttemptAndOneTerminalAudit(bool file)
    {
        using var fixture = new CourseRunFixture(file);
        await Prepare(fixture);
        var key = Guid.NewGuid();
        var attempts = await Task.WhenAll(Enumerable.Range(0, 4)
            .Select(_ => Service(fixture).StartAttemptAsync(Principal, 1, 8, key)));
        Assert.Single(attempts.Select(a => a.Id).Distinct());
        var first = attempts[0];
        var submitKey = Guid.NewGuid();
        var results = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ =>
            Service(fixture).SaveAttemptAsync(Principal, first.Id, Correct, true,
                OpaqueConcurrencyToken.ToETag(first.UpdateDate), submitKey)));
        Assert.All(results, result => Assert.Equal("2", result.Earned));
        Assert.Single((await fixture.Reader().ListAuditEventsByEntityAsync("SchoolBotAttempt", first.Id)), a => a.Event == "Attempt.Submitted");
    }

    [Fact]
    public async Task GetRecoversExpiredAttemptAndDoesNotLeakAnswersBeforeClose()
    {
        using var fixture = new CourseRunFixture(false);
        await Prepare(fixture);
        var first = await Service(fixture).StartAttemptAsync(Principal, 1, 7, Guid.NewGuid());
        Assert.Null(first.Feedback);
        Assert.Null(first.Earned);
        fixture.Clock.Advance(TimeSpan.FromSeconds(90));
        var expired = await Service(fixture).GetAttemptAsync(Principal, first.Id);
        Assert.Equal("expired", expired.State);
        Assert.Null(expired.Feedback);
        Assert.Null(expired.Earned);
        fixture.Clock.Advance(TimeSpan.FromHours(1));
        Assert.Equal("0", (await Service(fixture).GetAttemptAsync(Principal, first.Id)).Earned);
    }

    [Fact]
    public async Task RpcStartAndSubmitUsesTypedContract()
    {
        using var fixture = new CourseRunFixture(false);
        await Prepare(fixture);
        var student = await fixture.Student();
        var rpc = GrpcTest.Assessments(fixture);
        var request = new Rpc.StartAttemptRequest { CourseRunId = 1, ActivityId = 8 };
        request.IdempotencyKey.Add(Guid.NewGuid().ToString("D"));
        var started = await rpc.StartAttemptAsync(request, GrpcTest.Headers(student));
        Assert.Null(AssessmentRpc.ToModel(started).Feedback);
        var submit = new Rpc.SaveAttemptAnswersRequest { Id = started.Id, IfMatch = started.Etag };
        submit.IdempotencyKey.Add(Guid.NewGuid().ToString("D"));
        var answer = new Rpc.AssessmentAnswerItem { QuestionKey = Correct[0].QuestionKey.ToString("D") };
        answer.SelectedOptionKeys.Add(Correct[0].SelectedOptionKeys[0].ToString("D"));
        submit.Answers.Add(answer);
        var result = await rpc.SubmitAttemptAsync(submit, GrpcTest.Headers(student));
        Assert.Equal("submitted", result.State);
        Assert.Equal("2", result.Earned);
    }

    [Fact]
    public async Task StaleWriteAndUnknownQuestionCannotChangeSavedAnswers()
    {
        using var fixture = new CourseRunFixture(false);
        await Prepare(fixture);
        var first = await Service(fixture).StartAttemptAsync(Principal, 1, 8, Guid.NewGuid());
        var etag = OpaqueConcurrencyToken.ToETag(first.UpdateDate);
        await Service(fixture).SaveAttemptAsync(Principal, first.Id, Correct, false, etag, Guid.NewGuid());
        var stale = await Assert.ThrowsAsync<DomainRuleException>(() => Service(fixture)
            .SaveAttemptAsync(Principal, first.Id, [], false, etag, Guid.NewGuid()));
        Assert.Equal(ErrorCodes.VersionConflict, stale.Code);
        var invalid = await Assert.ThrowsAsync<DomainRuleException>(() => Service(fixture)
            .SaveAttemptAsync(Principal, first.Id, [new(Guid.NewGuid(), [])], false, etag, Guid.NewGuid()));
        Assert.Equal(ErrorCodes.InvalidInput, invalid.Code);
        Assert.Single((await Service(fixture).GetAttemptAsync(Principal, first.Id)).Answers);
    }

    [Fact]
    public void MissingEvaluatorNeverWritesZeroAndDeadlineUsesEarliestLimit()
    {
        var policy = QuestionBankSeed.Policy(false);
        var now = CourseRunSeed.Now;
        Assert.Equal(now.AddSeconds(10), AttemptEvaluation.Deadline(now, policy, now.AddSeconds(10)));
        var snapshot = new AttemptSnapshot(1, 1, 1, 1, "unknown", QuestionBankSeed.Questions(false), [], [], policy, null, null);
        Assert.Equal(ErrorCodes.GraderUnavailable,
            Assert.Throws<DomainRuleException>(() => AttemptEvaluation.Score(snapshot, Correct)).Code);
    }

    [Fact]
    public async Task SnapshotAndShuffleAreFrozenWhileStartingAgainRecoversPreviousExpiry()
    {
        using var fixture = new CourseRunFixture(false);
        await Prepare(fixture);
        var first = await Service(fixture).StartAttemptAsync(Principal, 1, 8, Guid.NewGuid());
        var frozen = (await fixture.Reader().GetAttemptByIdAsync(first.Id))!.Snapshot;
        await ((ICurriculumBackingStore)fixture.Store()).MutateCurriculumUnitAsync((snapshot, _) =>
            Task.FromResult(new CurriculumStoreMutation<bool>(snapshot with
            {
                QuestionBanks = snapshot.QuestionBanks!.Select(bank => bank with
                { Questions = QuestionBankSeed.Questions(true) }).ToArray()
            }, true)));
        fixture.Clock.Advance(TimeSpan.FromSeconds(120));
        var next = await Service(fixture).StartAttemptAsync(Principal, 1, 8, Guid.NewGuid());
        Assert.Equal(2, next.Sequence);
        var recovered = await Service(fixture).GetAttemptAsync(Principal, first.Id);
        Assert.Equal("expired", recovered.State);
        Assert.Equal(frozen, (await fixture.Reader().GetAttemptByIdAsync(first.Id))!.Snapshot);
        Assert.Equal(first.Questions.Select(q => q.QuestionKey), recovered.Questions.Select(q => q.QuestionKey));
    }

    [Fact]
    public async Task RevokedLicenseIsRecheckedBeforeReplayingSuccessfulMutation()
    {
        using var fixture = new CourseRunFixture(false);
        await Prepare(fixture);
        var student = await fixture.Student();
        var rpc = GrpcTest.Assessments(fixture);
        var request = new Rpc.StartAttemptRequest { CourseRunId = 1, ActivityId = 8 };
        request.IdempotencyKey.Add(Guid.NewGuid().ToString("D"));
        await rpc.StartAttemptAsync(request, GrpcTest.Headers(student));
        await fixture.Store().MutateLicenseUnitAsync(snapshot => new LicenseStoreMutation<bool>(
            snapshot with { Licenses = snapshot.Licenses.Select(l => l with { RevokedAt = fixture.Clock.GetUtcNow() }).ToArray() }, true));
        var denied = await GrpcTest.Fails(rpc.StartAttemptAsync(request, GrpcTest.Headers(student)));
        Assert.Equal(ErrorCodes.LicenseRequired, denied.Code);
        Assert.Single((await fixture.Snapshot()).Attempts!);
    }
}

