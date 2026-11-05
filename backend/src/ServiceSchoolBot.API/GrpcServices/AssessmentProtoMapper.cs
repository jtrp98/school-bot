using System.Globalization;
using System.Text.Json;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Domain.Assessment;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.API.GrpcServices;

// Absent optional fields map back to what SchoolBot's own binder produced for a missing member: null
// references, Guid.Empty and an undefined JsonElement.
internal static class AssessmentProtoMapper
{
    public static string Time(DateTimeOffset value) => value.ToString("O", CultureInfo.InvariantCulture);

    public static string? Time(DateTimeOffset? value) => value is { } time ? Time(time) : null;

    public static JsonElement Json(bool present, string text)
    {
        if (!present) return default;
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    public static IReadOnlyList<QuestionBankQuestion>? ToDomain(Rpc.QuestionBankQuestionList? questions) =>
        questions?.Items.Select(question => new QuestionBankQuestion(
            Guid.Parse(question.QuestionKey),
            question.HasKind ? question.Kind : null!,
            Json(question.HasPromptJson, question.PromptJson),
            question.Options?.Items.Select(ToDomain).ToList()!,
            question.CorrectOptionKeys?.Items.Select(Guid.Parse).ToList()!,
            question.HasMaxScore ? question.MaxScore : null!)).ToList();

    private static QuestionBankOption ToDomain(Rpc.QuestionBankOptionItem option) =>
        new(Guid.Parse(option.OptionKey), Json(option.HasTextJson, option.TextJson));

    public static Rpc.QuestionBankQuestionItem ToProto(QuestionBankQuestion question)
    {
        var result = new Rpc.QuestionBankQuestionItem
        {
            QuestionKey = question.QuestionKey.ToString("D"),
            Kind = question.Kind,
            PromptJson = question.Prompt.GetRawText(),
            Options = new Rpc.QuestionBankOptionList(),
            CorrectOptionKeys = new Rpc.KeyList(),
            MaxScore = question.MaxScore
        };
        result.Options.Items.AddRange(question.Options.Select(ToProto));
        result.CorrectOptionKeys.Items.AddRange(question.CorrectOptionKeys.Select(key => key.ToString("D")));
        return result;
    }

    private static Rpc.QuestionBankOptionItem ToProto(QuestionBankOption option) => new()
    {
        OptionKey = option.OptionKey.ToString("D"),
        TextJson = option.Text.GetRawText()
    };

    public static Rpc.AssessmentPolicyItem ToProto(AssessmentPolicy policy)
    {
        var result = new Rpc.AssessmentPolicyItem
        {
            Selection = policy.Selection,
            Reveal = policy.Reveal,
            ShuffleQuestions = policy.ShuffleQuestions,
            ShuffleOptions = policy.ShuffleOptions
        };
        if (policy.MaxAttempts is { } max) result.MaxAttempts = max;
        if (policy.TimeLimitSeconds is { } limit) result.TimeLimitSeconds = limit;
        return result;
    }

    public static Rpc.AssessmentApprovalItem? ToProto(CurriculumApprovalView? approval) => approval is null
        ? null
        : new Rpc.AssessmentApprovalItem
        {
            Actor = approval.Actor,
            ApprovedAt = Time(approval.ApprovedAt),
            ApprovalRef = approval.ApprovalRef
        };

    public static Rpc.QuestionBankResponse ToProto(QuestionBankDraftView view, string etag)
    {
        var result = new Rpc.QuestionBankResponse
        {
            Id = view.Id,
            BankKey = view.BankKey.ToString("D"),
            Revision = view.Revision,
            TitleJson = view.Title.GetRawText(),
            SourceRef = view.SourceRef,
            Lifecycle = view.Lifecycle,
            UpdateDate = Time(view.UpdateDate),
            Approval = ToProto(view.Approval),
            Etag = etag
        };
        if (Time(view.PublishedAt) is { } published) result.PublishedAt = published;
        result.Questions.AddRange(view.Questions.Select(ToProto));
        return result;
    }

    public static Rpc.QuestionBankListResponse ToProto(QuestionBankListResponse list)
    {
        var result = new Rpc.QuestionBankListResponse();
        if (list.NextCursor is not null) result.NextCursor = list.NextCursor;
        result.Items.AddRange(list.Items.Select(item =>
        {
            var summary = new Rpc.QuestionBankSummaryItem
            {
                Id = item.Id,
                BankKey = item.BankKey.ToString("D"),
                Revision = item.Revision,
                TitleJson = item.Title.GetRawText(),
                SourceRef = item.SourceRef,
                Lifecycle = item.Lifecycle,
                UpdateDate = Time(item.UpdateDate)
            };
            if (Time(item.PublishedAt) is { } published) summary.PublishedAt = published;
            return summary;
        }));
        return result;
    }

    public static Rpc.ActivityPolicyResponse ToProto(ActivityPolicyView view, string etag)
    {
        var result = new Rpc.ActivityPolicyResponse
        {
            CourseRunId = view.CourseRunId,
            ActivityId = view.ActivityId,
            EffectivePolicy = ToProto(view.EffectivePolicy),
            UpdateDate = Time(view.UpdateDate),
            Etag = etag
        };
        if (Time(view.OpensAt) is { } opens) result.OpensAt = opens;
        if (Time(view.ClosesAt) is { } closes) result.ClosesAt = closes;
        return result;
    }

    public static Rpc.AttemptResponse ToProto(AttemptView view, string etag)
    {
        var result = new Rpc.AttemptResponse
        {
            Id = view.Id,
            UpdateDate = Time(view.UpdateDate),
            Sequence = view.Sequence,
            State = view.State,
            ActivityId = view.ActivityId,
            CourseRunId = view.CourseRunId,
            StartedAt = Time(view.StartedAt),
            MaxScore = view.MaxScore,
            EffectivePolicy = ToProto(view.EffectivePolicy),
            BankId = view.BankId,
            BankRevision = view.BankRevision,
            CurriculumRevision = view.CurriculumRevision,
            PolicyId = view.PolicyId,
            EvaluatorVersion = view.EvaluatorVersion,
            ServerNow = Time(view.ServerNow),
            Etag = etag
        };
        if (Time(view.DeadlineAt) is { } deadline) result.DeadlineAt = deadline;
        if (Time(view.SubmittedAt) is { } submitted) result.SubmittedAt = submitted;
        if (view.Earned is not null) result.Earned = view.Earned;
        if (Time(view.OpensAt) is { } opens) result.OpensAt = opens;
        if (Time(view.ClosesAt) is { } closes) result.ClosesAt = closes;
        result.Questions.AddRange(view.Questions.Select(question =>
        {
            var rpc = new Rpc.PublicQuestionItem
            {
                QuestionKey = question.QuestionKey.ToString("D"),
                Kind = question.Kind,
                PromptJson = question.Prompt.GetRawText(),
                MaxScore = question.MaxScore
            };
            rpc.Options.AddRange(question.Options.Select(ToProto));
            return rpc;
        }));
        if (view.Feedback is not null)
        {
            result.Feedback = new Rpc.AttemptFeedbackList();
            result.Feedback.Items.AddRange(view.Feedback.Select(feedback =>
            {
                var rpc = new Rpc.AttemptFeedbackItem
                {
                    QuestionKey = feedback.QuestionKey.ToString("D"),
                    Correct = feedback.Correct
                };
                rpc.CorrectOptionKeys.AddRange(feedback.CorrectOptionKeys.Select(key => key.ToString("D")));
                return rpc;
            }));
        }

        result.Answers.AddRange(view.Answers.Select(answer =>
        {
            var rpc = new Rpc.AssessmentAnswerItem { QuestionKey = answer.QuestionKey.ToString("D") };
            rpc.SelectedOptionKeys.AddRange(answer.SelectedOptionKeys.Select(key => key.ToString("D")));
            return rpc;
        }));
        return result;
    }

    public static Rpc.AttemptListResponse ToProto(AttemptListResponse list)
    {
        var result = new Rpc.AttemptListResponse();
        if (list.NextCursor is not null) result.NextCursor = list.NextCursor;
        result.Items.AddRange(list.Items.Select(item =>
        {
            var summary = new Rpc.AttemptSummaryItem
            {
                Id = item.Id,
                UpdateDate = Time(item.UpdateDate),
                Sequence = item.Sequence,
                State = item.State,
                CourseRunId = item.CourseRunId,
                ActivityId = item.ActivityId,
                StartedAt = Time(item.StartedAt),
                MaxScore = item.MaxScore
            };
            if (Time(item.DeadlineAt) is { } deadline) summary.DeadlineAt = deadline;
            if (Time(item.SubmittedAt) is { } submitted) summary.SubmittedAt = submitted;
            if (item.Earned is not null) summary.Earned = item.Earned;
            return summary;
        }));
        return result;
    }

    public static Rpc.ProgressResponse ToProto(ProgressView view)
    {
        var result = new Rpc.ProgressResponse
        {
            LearnerId = view.LearnerId,
            CurriculumId = view.CurriculumId,
            LevelId = view.LevelId,
            ComputedAt = Time(view.ComputedAt)
        };
        if (view.CourseRunId is { } run) result.CourseRunId = run;
        result.Activities.AddRange(view.Activities.Select(activity =>
        {
            var rpc = new Rpc.ActivityProgressItem { ActivityId = activity.ActivityId, Type = activity.Type };
            if (activity.Mode is not null) rpc.Mode = activity.Mode;
            if (activity.LatestSubmission is { } submission) rpc.LatestSubmissionJson = submission.GetRawText();
            if (activity.SelectedAttempt is { } selected)
            {
                rpc.SelectedAttempt = new Rpc.SelectedAttemptItem
                {
                    AttemptId = selected.AttemptId,
                    Sequence = selected.Sequence,
                    State = selected.State,
                    MaxScore = selected.MaxScore
                };
                if (selected.Earned is not null) rpc.SelectedAttempt.Earned = selected.Earned;
            }

            return rpc;
        }));
        result.Missions.AddRange(view.Missions.Select(mission =>
        {
            var rpc = new Rpc.MissionProgressItem { MissionId = mission.MissionId };
            if (mission.LatestSubmission is { } submission) rpc.LatestSubmissionJson = submission.GetRawText();
            return rpc;
        }));
        return result;
    }
}
