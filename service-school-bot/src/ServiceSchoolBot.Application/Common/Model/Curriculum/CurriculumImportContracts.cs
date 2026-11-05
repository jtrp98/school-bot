using System.Text.Json.Serialization;

namespace ServiceSchoolBot.Application.Common.Model.Curriculum;

public sealed record CurriculumImportRequest(
    [property: JsonPropertyName("source_ref")] string SourceRef,
    [property: JsonPropertyName("rows")] IReadOnlyList<CurriculumImportRow> Rows);

public sealed record CurriculumImportRow(
    [property: JsonPropertyName("source_row_key")] Guid SourceRowKey,
    [property: JsonPropertyName("source_position")] int SourcePosition,
    [property: JsonPropertyName("source_approved"), JsonRequired] bool? SourceApproved,
    [property: JsonPropertyName("lesson")] LessonInput Lesson);

public sealed record CurriculumImportLessonResult(
    [property: JsonPropertyName("source_row_key")] Guid SourceRowKey,
    [property: JsonPropertyName("lesson_id")] int LessonId,
    [property: JsonPropertyName("outcome")] string Outcome);

public sealed record CurriculumImportResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("update_date")] DateTimeOffset UpdateDate,
    [property: JsonPropertyName("lessons")] IReadOnlyList<CurriculumImportLessonResult> Lessons);

public sealed record PreparedCurriculumImportRow(LessonInput Lesson, string ImportSource);
