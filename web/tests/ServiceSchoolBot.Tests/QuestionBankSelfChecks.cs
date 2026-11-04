using ServiceSchoolBot.Application.Common.Model.Assessment;
using System.Text.Json;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using Xunit.Abstractions;

namespace ServiceSchoolBot.Tests;

public sealed class QuestionBankSelfChecks(ITestOutputHelper output)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ActivityPolicyReadPreservesState(bool file)
    {
        using var fixture = new QuestionBankFixture(file);
        await fixture.Initialize();
        var client = GrpcTest.Assessments(fixture);
        var baseline = JsonSerializer.Serialize(await fixture.Snapshot());
        foreach (var actor in new[] { "student", "teacher" })
        {
            foreach (var id in new[] { 1, 2 })
            {
                var response = await client.GetActivityPolicyAsync(
                    new() { CourseRunId = 1, ActivityId = id }, QuestionBankFixture.As(actor));
                var raw = response.ToString();
                Assert.DoesNotContain("correct_option_keys", raw);
                Assert.DoesNotContain("correctOptionKeys", raw);
                var view = AssessmentRpc.ToModel(response);
                Assert.Equal(QuestionBankSeed.Policy(id == 1), view.EffectivePolicy);
                Assert.Equal(QuestionBankSeed.Start.AddHours(-1), view.OpensAt);
                Assert.Equal(QuestionBankSeed.Start.AddHours(1), view.ClosesAt);
                Assert.Equal(OpaqueConcurrencyToken.ToETag(view.UpdateDate), response.Etag);
                Assert.Equal(baseline, JsonSerializer.Serialize(await fixture.Snapshot()));
                var reopened = await fixture.ReopenedReader().GetActivityPolicyAsync(
                    QuestionBankFixture.Principal(actor), 1, id);
                Assert.Equal(view, reopened);
                Assert.Equal(baseline, JsonSerializer.Serialize(await fixture.Snapshot()));
            }
        }
        AssertCounts(await fixture.Snapshot(), 1, 2, 0);
        output.WriteLine("store={0}; seed_sha256={1}; policy reads=8; state unchanged", file ? "file" : "memory", fixture.SeedHash);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RevisionPublishPreservesAttemptSnapshotsAndResults(bool file)
    {
        using var fixture = new QuestionBankFixture(file);
        await fixture.Initialize();
        var client = GrpcTest.Assessments(fixture);
        var baseline = await fixture.Snapshot();
        var input = new CreateQuestionBankRequest(QuestionBankSeed.Key(200), 2,
            QuestionBankSeed.Text("คลังใหม่"), QuestionBankSeed.Questions(true), "fixture:bank-096-v2");
        var createdResponse = await client.CreateQuestionBankAsync(AssessmentRpc.Create(input, Guid.NewGuid()),
            QuestionBankFixture.As("author"));
        var created = AssessmentRpc.ToModel(createdResponse);
        Assert.Equal(2, created.Id);
        var tag = createdResponse.Etag;
        {
            var draft = AssessmentRpc.ToModel(
                await client.GetQuestionBankAsync(new() { Id = 2 }, QuestionBankFixture.As("author")));
            Assert.Equal("draft", draft.Lifecycle);
            Assert.Null(draft.Approval);
        }
        fixture.Clock.Advance(TimeSpan.FromSeconds(1));
        var updatedResponse = await client.UpdateQuestionBankAsync(AssessmentRpc.Update(2,
            new UpdateQuestionBankDraftRequest(QuestionBankSeed.Text("คลังใหม่ที่แก้ร่าง"), input.Questions, input.SourceRef),
            tag, Guid.NewGuid()), QuestionBankFixture.As("author"));
        tag = updatedResponse.Etag;
        fixture.Clock.Advance(TimeSpan.FromSeconds(1));
        await client.PublishQuestionBankAsync(AssessmentRpc.Publish(2,
            new PublishQuestionBankRequest("fixture:approved-v2"), tag, Guid.NewGuid()), QuestionBankFixture.As("approver"));
        {
            var detail = AssessmentRpc.ToModel(
                await client.GetQuestionBankAsync(new() { Id = 2 }, QuestionBankFixture.As("approver")));
            Assert.Equal("published", detail.Lifecycle);
            Assert.Equal("fixture:approved-v2", detail.Approval!.ApprovalRef);
            Assert.Equal("คลังใหม่ที่แก้ร่าง", detail.Title.GetProperty("th").GetString());
        }
        fixture.ReopenServices();
        var reader = fixture.Reader();
        var reopenedBanks = fixture.ReopenedBanks();
        Assert.Equal("published", (await reopenedBanks.GetQuestionBankAsync(QuestionBankFixture.Principal("author"), 1)).Lifecycle);
        Assert.Equal("published", (await reopenedBanks.GetQuestionBankAsync(QuestionBankFixture.Principal("author"), 2)).Lifecycle);
        Assert.Equal(2, (await reader.ListQuestionBanksAsync()).Count);
        foreach (var afterClose in new[] { false, true })
        {
            if (afterClose) fixture.Clock.Advance(TimeSpan.FromMinutes(70) - TimeSpan.FromSeconds(2));
            var detailResponse = await client.GetAttemptAsync(new() { Id = 1 }, QuestionBankFixture.As("student"));
            var detail = AssessmentRpc.ToModel(detailResponse);
            var reopened = await fixture.ReopenedReader().GetAttemptAsync(QuestionBankFixture.Principal("student"), 1);
            Assert.Equal(JsonSerializer.Serialize(detail), JsonSerializer.Serialize(reopened));
            Assert.Equal(afterClose ? "2" : null, detail.Earned);
            Assert.Equal("5", detail.MaxScore);
            Assert.Equal(1, detail.BankId);
            Assert.Equal(1, detail.BankRevision);
            Assert.Equal(1, detail.CurriculumRevision);
            Assert.Equal("fixture-evaluator-v1", detail.EvaluatorVersion);
            Assert.Equal(QuestionBankSeed.Policy(true), detail.EffectivePolicy);
            Assert.Equal(new[] { QuestionBankSeed.Key(202), QuestionBankSeed.Key(201) },
                detail.Questions.Select(question => question.QuestionKey));
            Assert.Equal(new[] { QuestionBankSeed.Key(212), QuestionBankSeed.Key(211) },
                detail.Questions[1].Options.Select(option => option.OptionKey));
            if (afterClose)
            {
                Assert.Equal(new[] { QuestionBankSeed.Key(221), QuestionBankSeed.Key(211) },
                    detail.Feedback!.SelectMany(feedback => feedback.CorrectOptionKeys));
                Assert.Equal(2m, EvaluateFrozenSnapshot((await reader.GetAttemptByIdAsync(1))!));
            }
            else Assert.Null(detail.Feedback);
            Assert.Equal(OpaqueConcurrencyToken.ToETag(baseline.Attempts![0].UpdateDate), detailResponse.Etag);
            var pending = await fixture.ReopenedReader().GetAttemptAsync(QuestionBankFixture.Principal("student"), 2);
            Assert.Equal("started", pending.State);
            Assert.Null(pending.Earned);
            Assert.Null(pending.Feedback);
            var list = AssessmentRpc.ToModel(
                await client.ListAttemptsAsync(new() { CourseRunId = 1 }, QuestionBankFixture.As("student")));
            Assert.Equal(new[] { 2, 1 }, list.Items.Select(item => item.Id));
            Assert.Equal(afterClose ? "2" : null, list.Items[1].Earned);
        }
        var final = await fixture.Snapshot();
        AssertCounts(final, 2, 5, 3);
        Assert.Equal(JsonSerializer.Serialize(baseline.Attempts), JsonSerializer.Serialize(final.Attempts));
        Assert.Equal(JsonSerializer.Serialize(baseline.Activities), JsonSerializer.Serialize(final.Activities));
        Assert.Equal(JsonSerializer.Serialize(baseline.CourseRuns), JsonSerializer.Serialize(final.CourseRuns));
        Assert.Equal(JsonSerializer.Serialize(baseline.CourseRunActivities), JsonSerializer.Serialize(final.CourseRunActivities));
        Assert.Equal(JsonSerializer.Serialize(baseline.QuestionBanks![0]), JsonSerializer.Serialize(final.QuestionBanks![0]));
        var audits = final.AuditEvents.Where(audit => audit.EntityId == 2).ToArray();
        Assert.Equal(new[] { "QuestionBank.Created", "QuestionBank.Updated", "QuestionBank.Published" },
            audits.Select(audit => audit.Event));
        Assert.Equal("fixture:approved-v2", audits[2].Reason);
        Assert.All(audits, audit => Assert.Null(audit.SchoolId));
        Assert.Equal(new[] { "create_question_bank", "update_question_bank_draft", "publish_question_bank" },
            final.Idempotency!.Select(entry => entry.Operation));
        Assert.Equal(JsonSerializer.Serialize(final), JsonSerializer.Serialize(await fixture.Snapshot()));
        output.WriteLine("store={0}; seed_sha256={1}; status=201/200/200/200; banks=2 audit=5 ledger=3; attempts unchanged",
            file ? "file" : "memory", fixture.SeedHash);
    }

    private static void AssertCounts(CurriculumStoreSnapshot snapshot, int banks, int audits, int ledger)
    {
        Assert.Equal(banks, snapshot.QuestionBanks!.Count);
        Assert.Single(snapshot.Curricula);
        Assert.Equal(2, snapshot.Lessons.Count);
        Assert.Equal(2, snapshot.Activities.Count);
        Assert.Equal(2, snapshot.Attempts!.Count);
        Assert.Equal(audits, snapshot.AuditEvents.Count);
        Assert.Equal(ledger, snapshot.Idempotency!.Count);
    }

    private static decimal EvaluateFrozenSnapshot(ServiceSchoolBot.Domain.Assessment.SchoolBotAttemptRecord attempt)
    {
        var snapshot = JsonSerializer.Deserialize<ServiceSchoolBot.Domain.Assessment.AttemptSnapshot>(attempt.Snapshot)!;
        var answers = JsonSerializer.Deserialize<ServiceSchoolBot.Domain.Assessment.AssessmentAnswer[]>(attempt.Answers)!;
        return snapshot.Questions.Where(question => answers.Any(answer => answer.QuestionKey == question.QuestionKey &&
            answer.SelectedOptionKeys.SequenceEqual(question.CorrectOptionKeys)))
            .Sum(question => decimal.Parse(question.MaxScore, System.Globalization.CultureInfo.InvariantCulture));
    }
}
