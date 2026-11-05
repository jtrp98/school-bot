using System.Globalization;
using System.Text.Json;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Domain.Assessment;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.Tests;

// Converts between the assessment records the tests assert on and the AssessmentService messages, the same
// way the Gateway translates JSON bodies.
internal static class AssessmentRpc
{
    private static DateTimeOffset Time(string value) => DateTimeOffset.ParseExact(value, "O", CultureInfo.InvariantCulture);

    private static DateTimeOffset? Time(bool present, string value) => present ? Time(value) : null;


    private static JsonElement Json(string text)
    {
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    private static Rpc.QuestionBankQuestionList Questions(IEnumerable<QuestionBankQuestion> questions)
    {
        var list = new Rpc.QuestionBankQuestionList();
        list.Items.AddRange(questions.Select(question =>
        {
            var item = new Rpc.QuestionBankQuestionItem
            {
                QuestionKey = question.QuestionKey.ToString("D"),
                Kind = question.Kind,
                PromptJson = question.Prompt.GetRawText(),
                Options = new Rpc.QuestionBankOptionList(),
                CorrectOptionKeys = new Rpc.KeyList(),
                MaxScore = question.MaxScore
            };
            item.Options.Items.AddRange(question.Options.Select(option => new Rpc.QuestionBankOptionItem
            {
                OptionKey = option.OptionKey.ToString("D"),
                TextJson = option.Text.GetRawText()
            }));
            item.CorrectOptionKeys.Items.AddRange(question.CorrectOptionKeys.Select(key => key.ToString("D")));
            return item;
        }));
        return list;
    }

    public static Rpc.CreateQuestionBankRequest Create(CreateQuestionBankRequest body, Guid idempotencyKey)
    {
        var request = new Rpc.CreateQuestionBankRequest
        {
            BankKey = body.BankKey.ToString("D"),
            Revision = body.Revision,
            TitleJson = body.Title.GetRawText(),
            Questions = Questions(body.Questions),
            SourceRef = body.SourceRef
        };
        request.IdempotencyKey.Add(idempotencyKey.ToString("D"));
        return request;
    }

    public static Rpc.UpdateQuestionBankRequest Update(int id, UpdateQuestionBankDraftRequest body, string ifMatch,
        Guid idempotencyKey)
    {
        var request = new Rpc.UpdateQuestionBankRequest
        {
            Id = id,
            IfMatch = ifMatch,
            TitleJson = body.Title.GetRawText(),
            Questions = Questions(body.Questions),
            SourceRef = body.SourceRef
        };
        request.IdempotencyKey.Add(idempotencyKey.ToString("D"));
        return request;
    }

    public static Rpc.PublishQuestionBankRequest Publish(int id, PublishQuestionBankRequest body, string ifMatch,
        Guid idempotencyKey)
    {
        var request = new Rpc.PublishQuestionBankRequest { Id = id, IfMatch = ifMatch };
        if (body.ApprovalRef is not null) request.ApprovalRef = body.ApprovalRef;
        request.IdempotencyKey.Add(idempotencyKey.ToString("D"));
        return request;
    }

    public static QuestionBankMutationResponse ToModel(Rpc.QuestionBankMutationResponse response) =>
        new(response.Id, Time(response.UpdateDate), Time(response.HasPublishedAt, response.PublishedAt));

    private static AssessmentPolicy ToModel(Rpc.AssessmentPolicyItem policy) =>
        new(policy.HasMaxAttempts ? policy.MaxAttempts : null, policy.Selection,
            policy.HasTimeLimitSeconds ? policy.TimeLimitSeconds : null, policy.Reveal, policy.ShuffleQuestions,
            policy.ShuffleOptions);

    private static QuestionBankOption ToModel(Rpc.QuestionBankOptionItem option) =>
        new(Guid.Parse(option.OptionKey), Json(option.TextJson));

    public static QuestionBankDraftView ToModel(Rpc.QuestionBankResponse response) =>
        new(response.Id, Guid.Parse(response.BankKey), response.Revision, Json(response.TitleJson),
            response.Questions.Select(question => new QuestionBankQuestion(Guid.Parse(question.QuestionKey),
                question.Kind, Json(question.PromptJson), question.Options.Items.Select(ToModel).ToList(),
                question.CorrectOptionKeys.Items.Select(Guid.Parse).ToList(), question.MaxScore)).ToList(),
            response.SourceRef, response.Lifecycle, Time(response.HasPublishedAt, response.PublishedAt),
            Time(response.UpdateDate), response.Approval is { } approval
                ? new CurriculumApprovalView(approval.Actor, Time(approval.ApprovedAt), approval.ApprovalRef)
                : null);

    public static QuestionBankListResponse ToModel(Rpc.QuestionBankListResponse response) =>
        new(response.Items.Select(item => new QuestionBankSummary(item.Id, Guid.Parse(item.BankKey), item.Revision,
                Json(item.TitleJson), item.SourceRef, item.Lifecycle, Time(item.HasPublishedAt, item.PublishedAt),
                Time(item.UpdateDate))).ToList(),
            response.HasNextCursor ? response.NextCursor : null);

    public static ActivityPolicyView ToModel(Rpc.ActivityPolicyResponse response) =>
        new(response.CourseRunId, response.ActivityId, Time(response.HasOpensAt, response.OpensAt),
            Time(response.HasClosesAt, response.ClosesAt), ToModel(response.EffectivePolicy), Time(response.UpdateDate));

    public static AttemptView ToModel(Rpc.AttemptResponse response) =>
        new(response.Id, Time(response.UpdateDate), response.Sequence, response.State, response.ActivityId,
            response.CourseRunId, Time(response.StartedAt), Time(response.HasDeadlineAt, response.DeadlineAt),
            Time(response.HasSubmittedAt, response.SubmittedAt),
            response.Questions.Select(question => new PublicQuestion(Guid.Parse(question.QuestionKey), question.Kind,
                Json(question.PromptJson), question.Options.Select(ToModel).ToList(), question.MaxScore)).ToList(),
            response.HasEarned ? response.Earned : null, response.MaxScore,
            response.Feedback?.Items.Select(feedback => new AttemptFeedback(Guid.Parse(feedback.QuestionKey),
                feedback.Correct, feedback.CorrectOptionKeys.Select(Guid.Parse).ToList())).ToList(),
            response.Answers.Select(answer => new AssessmentAnswer(Guid.Parse(answer.QuestionKey),
                answer.SelectedOptionKeys.Select(Guid.Parse).ToList())).ToList(),
            ToModel(response.EffectivePolicy), Time(response.HasOpensAt, response.OpensAt),
            Time(response.HasClosesAt, response.ClosesAt), response.BankId, response.BankRevision,
            response.CurriculumRevision, response.PolicyId, response.EvaluatorVersion, Time(response.ServerNow));

    public static AttemptListResponse ToModel(Rpc.AttemptListResponse response) =>
        new(response.Items.Select(item => new AttemptSummary(item.Id, Time(item.UpdateDate), item.Sequence, item.State,
                item.CourseRunId, item.ActivityId, Time(item.StartedAt), Time(item.HasDeadlineAt, item.DeadlineAt),
                Time(item.HasSubmittedAt, item.SubmittedAt), item.HasEarned ? item.Earned : null, item.MaxScore)).ToList(),
            response.HasNextCursor ? response.NextCursor : null);

    public static ProgressView ToModel(Rpc.ProgressResponse response) =>
        new(response.LearnerId, response.HasCourseRunId ? response.CourseRunId : null, response.CurriculumId,
            response.LevelId,
            response.Activities.Select(activity => new ActivityProgressView(activity.ActivityId, activity.Type,
                activity.HasMode ? activity.Mode : null,
                activity.HasLatestSubmissionJson ? Json(activity.LatestSubmissionJson) : null,
                activity.SelectedAttempt is { } selected
                    ? new SelectedAttemptView(selected.AttemptId, selected.Sequence, selected.State,
                        selected.HasEarned ? selected.Earned : null, selected.MaxScore)
                    : null)).ToList(),
            response.Missions.Select(mission => new MissionProgressView(mission.MissionId,
                mission.HasLatestSubmissionJson ? Json(mission.LatestSubmissionJson) : null)).ToList(),
            Time(response.ComputedAt));
}
