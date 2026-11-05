using System.Text.Json.Serialization;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Common.Model.Curriculum;

public sealed record CurriculumMetadataInput(
    [property: JsonPropertyName("level_id")] int LevelId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("source_ref")] string SourceRef,
    [property: JsonPropertyName("recommended_grades")] IReadOnlyList<string> RecommendedGrades,
    [property: JsonPropertyName("subjects")] IReadOnlyList<CurriculumSubject> Subjects
);

public sealed record CreateCurriculumRequest(
    [property: JsonPropertyName("curriculum_key")] Guid CurriculumKey,
    [property: JsonPropertyName("revision")] int Revision,
    [property: JsonPropertyName("level_id")] int LevelId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("source_ref")] string SourceRef,
    [property: JsonPropertyName("recommended_grades")] IReadOnlyList<string> RecommendedGrades,
    [property: JsonPropertyName("subjects")] IReadOnlyList<CurriculumSubject> Subjects
);

public sealed record CriteriaInput(
    [property: JsonPropertyName("revision")] int Revision,
    [property: JsonPropertyName("rules")] string Rules,
    [property: JsonPropertyName("source_ref")] string SourceRef,
    [property: JsonPropertyName("approved_by")] string? ApprovedBy = null,
    [property: JsonPropertyName("approved_at")] DateTimeOffset? ApprovedAt = null
);

public sealed record MissionInput(
    [property: JsonPropertyName("mission_key")] Guid MissionKey,
    [property: JsonPropertyName("ordinal")] int Ordinal,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("instructions")] string Instructions,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("criteria")] CriteriaInput Criteria
);

public sealed record ActivityInput(
    [property: JsonPropertyName("activity_key")] Guid ActivityKey,
    [property: JsonPropertyName("ordinal")] int Ordinal,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("mode")] string? Mode = null,
    [property: JsonPropertyName("indicators")] string? Indicators = null,
    [property: JsonPropertyName("config")] string? Config = null
);

public sealed record LessonInput(
    [property: JsonPropertyName("lesson_key")] Guid LessonKey,
    [property: JsonPropertyName("ordinal")] int Ordinal,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("objective")] string Objective,
    [property: JsonPropertyName("instructions")] string Instructions,
    [property: JsonPropertyName("missions")] IReadOnlyList<MissionInput> Missions,
    [property: JsonPropertyName("activities")] IReadOnlyList<ActivityInput> Activities
);

public sealed record UpdateCurriculumContentRequest(
    [property: JsonPropertyName("lessons")] IReadOnlyList<LessonInput> Lessons
);

public sealed record PublishCurriculumRequest(
    [property: JsonPropertyName("approval_ref")] string? ApprovalRef
);

public sealed record CurriculumApprovalView(
    [property: JsonPropertyName("actor")] string Actor,
    [property: JsonPropertyName("approved_at")] DateTimeOffset ApprovedAt,
    [property: JsonPropertyName("approval_ref")] string ApprovalRef
);

public sealed record CurriculumSummary(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("curriculum_key")] Guid CurriculumKey,
    [property: JsonPropertyName("revision")] int Revision,
    [property: JsonPropertyName("level_id")] int LevelId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("lifecycle")] string Lifecycle,
    [property: JsonPropertyName("source_ref")] string SourceRef,
    [property: JsonPropertyName("published_at")] DateTimeOffset? PublishedAt,
    [property: JsonPropertyName("update_date")] DateTimeOffset UpdateDate
);

public sealed record CurriculumDraftView(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("curriculum_key")] Guid CurriculumKey,
    [property: JsonPropertyName("revision")] int Revision,
    [property: JsonPropertyName("level_id")] int LevelId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("lifecycle")] string Lifecycle,
    [property: JsonPropertyName("source_ref")] string SourceRef,
    [property: JsonPropertyName("published_at")] DateTimeOffset? PublishedAt,
    [property: JsonPropertyName("update_date")] DateTimeOffset UpdateDate,
    [property: JsonPropertyName("recommended_grades")] IReadOnlyList<string> RecommendedGrades,
    [property: JsonPropertyName("subjects")] IReadOnlyList<CurriculumSubject> Subjects,
    [property: JsonPropertyName("lessons")] IReadOnlyList<LessonInput> Lessons,
    [property: JsonPropertyName("approval")] CurriculumApprovalView? Approval
);

public sealed record CurriculumMutationResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("update_date")] DateTimeOffset UpdateDate,
    [property: JsonPropertyName("published_at")] DateTimeOffset? PublishedAt = null
);

public sealed record CurriculumListResponse(
    [property: JsonPropertyName("items")] IReadOnlyList<CurriculumSummary> Items,
    [property: JsonPropertyName("next_cursor")] string? NextCursor = null
);
