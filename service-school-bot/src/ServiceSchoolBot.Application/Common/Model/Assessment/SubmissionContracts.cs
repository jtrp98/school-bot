using System.Text.Json.Serialization;
using ServiceSchoolBot.Domain.Assessment;

namespace ServiceSchoolBot.Application.Common.Model.Assessment;

public sealed record CreateSubmissionRequest(
    [property: JsonPropertyName("mission_id")] int MissionId,
    [property: JsonPropertyName("criteria_id")] int CriteriaId,
    [property: JsonPropertyName("project_revision_id")] int ProjectRevisionId,
    [property: JsonPropertyName("evidence")] string? Evidence = null);

public sealed record ReviewSubmissionRequest(
    [property: JsonPropertyName("outcome")] string Outcome,
    [property: JsonPropertyName("feedback")] string Feedback,
    [property: JsonPropertyName("rule_results")] IReadOnlyList<LearningRuleResult> RuleResults,
    [property: JsonPropertyName("supersedes_id")] int? SupersedesId = null,
    [property: JsonPropertyName("reason")] string? Reason = null);

public sealed record SubmissionView(
    [property: JsonPropertyName("submission")] SchoolBotSubmissionRecord Submission,
    [property: JsonPropertyName("assessment")] SchoolBotAssessmentRecord? Assessment,
    string? ProjectContent = null, string? ProjectMode = null);
