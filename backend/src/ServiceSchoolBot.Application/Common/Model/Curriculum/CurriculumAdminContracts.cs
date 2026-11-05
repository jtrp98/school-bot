using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using System.Text.Json.Serialization;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Common.Model.Curriculum;

public sealed record AdminLessonView(
    [property: JsonPropertyName("lesson_key")] Guid LessonKey,
    [property: JsonPropertyName("ordinal")] int Ordinal,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("objective")] string Objective,
    [property: JsonPropertyName("instructions")] string Instructions,
    [property: JsonPropertyName("missions")] IReadOnlyList<MissionInput> Missions,
    [property: JsonPropertyName("activities")] IReadOnlyList<ActivityInput> Activities,
    [property: JsonPropertyName("import_source")] LessonImportSource? ImportSource
);

public sealed record CurriculumAdminView(
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
    [property: JsonPropertyName("lessons")] IReadOnlyList<AdminLessonView> Lessons,
    [property: JsonPropertyName("approval")] CurriculumApprovalView? Approval
);
