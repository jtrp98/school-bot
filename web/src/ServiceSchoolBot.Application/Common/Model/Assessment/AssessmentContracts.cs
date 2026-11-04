using ServiceSchoolBot.Application.Common.Model.Curriculum;
using System.Text.Json;
using System.Text.Json.Serialization;
using ServiceSchoolBot.Domain.Assessment;

namespace ServiceSchoolBot.Application.Common.Model.Assessment;

public sealed record AssessmentPrincipalContext(
    CurriculumPrincipalContext Session,
    int? EmployeeId = null,
    string? ProfileKind = null,
    int? LearnerId = null);

public interface IAssessmentAuthority
{
    Task<bool> CanReadRunAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run,
        CancellationToken ct = default);
}

public sealed class UnconfiguredAssessmentAuthority : IAssessmentAuthority
{
    public Task<bool> CanReadRunAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run,
        CancellationToken ct = default) => Task.FromResult(false);
}

public sealed record CreateQuestionBankRequest(
    [property: JsonPropertyName("bank_key")] Guid BankKey,
    [property: JsonPropertyName("revision")] int Revision,
    [property: JsonPropertyName("title")] JsonElement Title,
    [property: JsonPropertyName("questions")] IReadOnlyList<QuestionBankQuestion> Questions,
    [property: JsonPropertyName("source_ref")] string SourceRef);

public sealed record UpdateQuestionBankDraftRequest(
    [property: JsonPropertyName("title")] JsonElement Title,
    [property: JsonPropertyName("questions")] IReadOnlyList<QuestionBankQuestion> Questions,
    [property: JsonPropertyName("source_ref")] string SourceRef);

public sealed record PublishQuestionBankRequest(
    [property: JsonPropertyName("approval_ref")] string? ApprovalRef);

public sealed record QuestionBankMutationResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("update_date")] DateTimeOffset UpdateDate,
    [property: JsonPropertyName("published_at"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    DateTimeOffset? PublishedAt = null);

public sealed record QuestionBankSummary(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("bank_key")] Guid BankKey,
    [property: JsonPropertyName("revision")] int Revision,
    [property: JsonPropertyName("title")] JsonElement Title,
    [property: JsonPropertyName("source_ref")] string SourceRef,
    [property: JsonPropertyName("lifecycle")] string Lifecycle,
    [property: JsonPropertyName("published_at")] DateTimeOffset? PublishedAt,
    [property: JsonPropertyName("update_date")] DateTimeOffset UpdateDate);

public sealed record QuestionBankListResponse(
    [property: JsonPropertyName("items")] IReadOnlyList<QuestionBankSummary> Items,
    [property: JsonPropertyName("next_cursor")] string? NextCursor);

public sealed record QuestionBankDraftView(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("bank_key")] Guid BankKey,
    [property: JsonPropertyName("revision")] int Revision,
    [property: JsonPropertyName("title")] JsonElement Title,
    [property: JsonPropertyName("questions")] IReadOnlyList<QuestionBankQuestion> Questions,
    [property: JsonPropertyName("source_ref")] string SourceRef,
    [property: JsonPropertyName("lifecycle")] string Lifecycle,
    [property: JsonPropertyName("published_at")] DateTimeOffset? PublishedAt,
    [property: JsonPropertyName("update_date")] DateTimeOffset UpdateDate,
    [property: JsonPropertyName("approval")] CurriculumApprovalView? Approval);

public sealed record ActivityPolicyView(
    [property: JsonPropertyName("course_run_id")] int CourseRunId,
    [property: JsonPropertyName("activity_id")] int ActivityId,
    [property: JsonPropertyName("opens_at")] DateTimeOffset? OpensAt,
    [property: JsonPropertyName("closes_at")] DateTimeOffset? ClosesAt,
    [property: JsonPropertyName("effective_policy")] AssessmentPolicy EffectivePolicy,
    [property: JsonPropertyName("update_date")] DateTimeOffset UpdateDate);

public sealed record PublicQuestion(
    [property: JsonPropertyName("question_key")] Guid QuestionKey,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("prompt")] JsonElement Prompt,
    [property: JsonPropertyName("options")] IReadOnlyList<QuestionBankOption> Options,
    [property: JsonPropertyName("max_score")] string MaxScore);

public sealed record AttemptFeedback(
    [property: JsonPropertyName("question_key")] Guid QuestionKey,
    [property: JsonPropertyName("correct")] bool Correct,
    [property: JsonPropertyName("correct_option_keys")] IReadOnlyList<Guid> CorrectOptionKeys);

public sealed record AttemptSummary(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("update_date")] DateTimeOffset UpdateDate,
    [property: JsonPropertyName("sequence")] int Sequence,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("course_run_id")] int CourseRunId,
    [property: JsonPropertyName("activity_id")] int ActivityId,
    [property: JsonPropertyName("started_at")] DateTimeOffset StartedAt,
    [property: JsonPropertyName("deadline_at")] DateTimeOffset? DeadlineAt,
    [property: JsonPropertyName("submitted_at")] DateTimeOffset? SubmittedAt,
    [property: JsonPropertyName("earned")] string? Earned,
    [property: JsonPropertyName("max_score")] string MaxScore);

public sealed record AttemptListResponse(
    [property: JsonPropertyName("items")] IReadOnlyList<AttemptSummary> Items,
    [property: JsonPropertyName("next_cursor")] string? NextCursor);

public sealed record AttemptView(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("update_date")] DateTimeOffset UpdateDate,
    [property: JsonPropertyName("sequence")] int Sequence,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("activity_id")] int ActivityId,
    [property: JsonPropertyName("course_run_id")] int CourseRunId,
    [property: JsonPropertyName("started_at")] DateTimeOffset StartedAt,
    [property: JsonPropertyName("deadline_at")] DateTimeOffset? DeadlineAt,
    [property: JsonPropertyName("submitted_at")] DateTimeOffset? SubmittedAt,
    [property: JsonPropertyName("questions")] IReadOnlyList<PublicQuestion> Questions,
    [property: JsonPropertyName("earned")] string? Earned,
    [property: JsonPropertyName("max_score")] string MaxScore,
    [property: JsonPropertyName("feedback")] IReadOnlyList<AttemptFeedback>? Feedback,
    [property: JsonPropertyName("answers")] IReadOnlyList<AssessmentAnswer> Answers,
    [property: JsonPropertyName("effective_policy")] AssessmentPolicy EffectivePolicy,
    [property: JsonPropertyName("opens_at")] DateTimeOffset? OpensAt,
    [property: JsonPropertyName("closes_at")] DateTimeOffset? ClosesAt,
    [property: JsonPropertyName("bank_id")] int BankId,
    [property: JsonPropertyName("bank_revision")] int BankRevision,
    [property: JsonPropertyName("curriculum_revision")] int CurriculumRevision,
    [property: JsonPropertyName("policy_id")] int PolicyId,
    [property: JsonPropertyName("evaluator_version")] string EvaluatorVersion,
    [property: JsonPropertyName("server_now")] DateTimeOffset ServerNow);
