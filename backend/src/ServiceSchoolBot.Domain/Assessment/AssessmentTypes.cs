using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceSchoolBot.Domain.Assessment;

public sealed record QuestionBankOption(
    [property: JsonPropertyName("option_key")] Guid OptionKey,
    [property: JsonPropertyName("text")] JsonElement Text);

public sealed record QuestionBankQuestion(
    [property: JsonPropertyName("question_key")] Guid QuestionKey,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("prompt")] JsonElement Prompt,
    [property: JsonPropertyName("options")] IReadOnlyList<QuestionBankOption> Options,
    [property: JsonPropertyName("correct_option_keys")] IReadOnlyList<Guid> CorrectOptionKeys,
    [property: JsonPropertyName("max_score")] string MaxScore);

public sealed record AssessmentPolicy(
    [property: JsonPropertyName("max_attempts")] int? MaxAttempts,
    [property: JsonPropertyName("selection")] string Selection,
    [property: JsonPropertyName("time_limit_seconds")] int? TimeLimitSeconds,
    [property: JsonPropertyName("reveal")] string Reveal,
    [property: JsonPropertyName("shuffle_questions")] bool ShuffleQuestions,
    [property: JsonPropertyName("shuffle_options")] bool ShuffleOptions);

public sealed record AssessmentAnswer(
    [property: JsonPropertyName("question_key")] Guid QuestionKey,
    [property: JsonPropertyName("selected_option_keys")] IReadOnlyList<Guid> SelectedOptionKeys);

public sealed record AssessmentOptionOrder(
    [property: JsonPropertyName("question_key")] Guid QuestionKey,
    [property: JsonPropertyName("option_keys")] IReadOnlyList<Guid> OptionKeys);

public sealed record AttemptSnapshot(
    [property: JsonPropertyName("bank_id")] int BankId,
    [property: JsonPropertyName("bank_revision")] int BankRevision,
    [property: JsonPropertyName("curriculum_revision")] int CurriculumRevision,
    [property: JsonPropertyName("policy_id")] int PolicyId,
    [property: JsonPropertyName("evaluator_version")] string EvaluatorVersion,
    [property: JsonPropertyName("questions")] IReadOnlyList<QuestionBankQuestion> Questions,
    [property: JsonPropertyName("question_order")] IReadOnlyList<Guid> QuestionOrder,
    [property: JsonPropertyName("option_order")] IReadOnlyList<AssessmentOptionOrder> OptionOrder,
    [property: JsonPropertyName("effective_policy")] AssessmentPolicy EffectivePolicy,
    [property: JsonPropertyName("opens_at")] DateTimeOffset? OpensAt,
    [property: JsonPropertyName("closes_at")] DateTimeOffset? ClosesAt);
