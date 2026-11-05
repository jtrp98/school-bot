using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceSchoolBot.Application.Common.Model.Assessment;

public sealed record ProgressView(
    [property: JsonPropertyName("learner_id")] int LearnerId,
    [property: JsonPropertyName("course_run_id")] int? CourseRunId,
    [property: JsonPropertyName("curriculum_id")] int CurriculumId,
    [property: JsonPropertyName("level_id")] int LevelId,
    [property: JsonPropertyName("activities")] IReadOnlyList<ActivityProgressView> Activities,
    [property: JsonPropertyName("missions")] IReadOnlyList<MissionProgressView> Missions,
    [property: JsonPropertyName("computed_at")] DateTimeOffset ComputedAt);

public sealed record ActivityProgressView(
    [property: JsonPropertyName("activity_id")] int ActivityId,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("mode")] string? Mode,
    [property: JsonPropertyName("latest_submission")] JsonElement? LatestSubmission,
    [property: JsonPropertyName("selected_attempt")] SelectedAttemptView? SelectedAttempt);

public sealed record MissionProgressView(
    [property: JsonPropertyName("mission_id")] int MissionId,
    [property: JsonPropertyName("latest_submission")] JsonElement? LatestSubmission);

public sealed record SelectedAttemptView(
    [property: JsonPropertyName("attempt_id")] int AttemptId,
    [property: JsonPropertyName("sequence")] int Sequence,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("earned")] string? Earned,
    [property: JsonPropertyName("max_score")] string MaxScore);
