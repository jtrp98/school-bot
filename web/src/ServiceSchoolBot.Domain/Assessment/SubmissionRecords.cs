using System.Text.Json.Serialization;

namespace ServiceSchoolBot.Domain.Assessment;

public sealed record SchoolBotSubmissionRecord(int Id, int SchoolId, int LearnerId, int MissionId,
    int CriteriaId, int ProjectRevisionId, DateTimeOffset SubmittedAt, string State,
    int? CurrentAssessmentId, string? Evidence, DateTimeOffset UpdateDate);

public sealed record SchoolBotAssessmentRecord(int Id, int SchoolId, int SubmissionId, int Sequence,
    string Method, string Outcome, decimal? EarnedScore, decimal? MaxScore, string RuleResults,
    string Feedback, string? GraderVersion, int? AssessorId, int? SupersedesId, DateTimeOffset CreatedAt);

public sealed record SchoolBotGradingJobRecord(int Id, int SchoolId, int SubmissionId, string State,
    int Attempts, string? LeaseOwner, DateTimeOffset? LeaseUntil, DateTimeOffset NextAttemptAt, string? LastErrorCode);

public sealed record LearningCommand(string Op, int? Value = null);
public sealed record LearningRuleResult([property: JsonPropertyName("rule_id")] Guid RuleId,
    [property: JsonPropertyName("matched")] bool Matched);
public sealed record LearningEvaluation(string Outcome, IReadOnlyList<LearningRuleResult> Rules, string Version);

public sealed record SchoolBotRetakeGrantRecord(int Id, int SchoolId, int CourseRunId, int ActivityId,
    int LearnerId, string Reason, int? ConsumedAttemptId, int EmployeeId, DateTimeOffset CreatedAt);
public sealed record ScoringActivityWeight([property: JsonPropertyName("activity_id")] int ActivityId,
    [property: JsonPropertyName("weight_bps")] int WeightBps);
public sealed record ScoringPolicy([property: JsonPropertyName("quiz_counts")] bool QuizCounts,
    [property: JsonPropertyName("activity_weights")] IReadOnlyList<ScoringActivityWeight> ActivityWeights,
    [property: JsonPropertyName("formula_version")] string FormulaVersion);
public sealed record SchoolBotScoringPolicyRecord(int Id, int SchoolId, int CourseRunId, int Revision,
    ScoringPolicy Policy, int EmployeeId, DateTimeOffset CreatedAt);
