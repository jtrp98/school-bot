using ServiceSchoolBot.Application.Common.Model.Curriculum;
using System.Text.Json;
using System.Text.Json.Serialization;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Services.Implementations.Curriculum;

public sealed partial class CurriculumService
{
    public async Task<CurriculumImportResponse> ImportCurriculumLessonsAsync(
        CurriculumPrincipalContext principal,
        int id,
        CurriculumImportRequest request,
        Guid? requestId = null,
        Guid? idempotencyKey = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(request);
        var access = CurriculumAuthorizationService.EvaluateUpdateCurriculumContent(principal, clock.GetUtcNow());
        if (!access.IsAllowed)
            throw new DomainRuleException(access.ErrorCode!, access.Message!);
        var rows = PrepareImportRows(request);
        var requestHash = ComputeSha256($"{id}|{JsonSerializer.Serialize(request)}");
        return await CommitMutationAsync(principal, "import_curriculum_lessons", requestHash,
            idempotencyKey, 200, (transaction, now, token) =>
            {
                var audit = new SchoolBotAuditEventRecord(0, principal.PrincipalId!,
                    "Curriculum.LessonsImported", "SchoolBotCurriculum", id, requestId ?? Guid.NewGuid(),
                    "Curriculum lessons imported", false, null, now, null, now, null);
                return transaction.ImportCurriculumLessonsAsync(id, rows, audit, token);
            }, response => (response.Id, response.UpdateDate), ct);
    }

    private static IReadOnlyList<PreparedCurriculumImportRow> PrepareImportRows(CurriculumImportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SourceRef) || request.SourceRef.Length > MaxSourceRefLength)
            throw ImportInputError(null, "source_ref");
        if (request.Rows is null || request.Rows.Count == 0)
            throw ImportInputError(null, "rows");
        var keys = new HashSet<Guid>();
        var ordinals = new HashSet<int>();
        var prepared = new List<PreparedCurriculumImportRow>();
        foreach (var row in request.Rows)
        {
            if (row is null) throw ImportInputError(null, "rows");
            if (row.SourceRowKey == Guid.Empty || !keys.Add(row.SourceRowKey))
                throw ImportInputError(row, "source_row_key");
            if (row.SourcePosition < 1) throw ImportInputError(row, "source_position");
            var lesson = row.Lesson;
            if (lesson is null) throw ImportInputError(row, "lesson");
            if (lesson.LessonKey != row.SourceRowKey) throw ImportInputError(row, "lesson.lesson_key");
            if (!ordinals.Add(lesson.Ordinal)) throw ImportInputError(row, "lesson.ordinal");
            if (lesson.Missions?.Any(mission => mission is null) == true)
                throw ImportInputError(row, "lesson.missions");
            if (lesson.Activities?.Any(activity => activity is null) == true)
                throw ImportInputError(row, "lesson.activities");
            try { ValidateContentInput(new UpdateCurriculumContentRequest([lesson])); }
            catch (DomainRuleException) { throw ImportInputError(row, "lesson"); }
            foreach (var activity in lesson.Activities ?? [])
                ValidateImportMedia(row, activity);
            var payload = new RowHashPayload(request.SourceRef, row.SourceRowKey.ToString("D"),
                row.SourceApproved, lesson.Title, lesson.Objective, lesson.Instructions,
                lesson.Missions, lesson.Activities);
            var hash = ComputeSha256(JsonSerializer.Serialize(payload));
            var source = new LessonImportSource(request.SourceRef, row.SourceRowKey.ToString("D"),
                row.SourceApproved, hash);
            prepared.Add(new PreparedCurriculumImportRow(lesson, LessonImportSourceCodec.Encode(source, lesson.LessonKey)));
        }
        return prepared;
    }

    private static void ValidateImportMedia(CurriculumImportRow row, ActivityInput activity)
    {
        if (activity.Config is null) return;
        try
        {
            using var document = JsonDocument.Parse(activity.Config);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                throw ImportInputError(row, "lesson.activities.config");
            if (!root.TryGetProperty("kind", out var kind) || kind.ValueKind != JsonValueKind.String ||
                kind.GetString() != "instruction-v1") return;
            if (!root.TryGetProperty("media_refs", out var media) || media.ValueKind != JsonValueKind.Array)
                throw ImportInputError(row, "lesson.activities.config.media_refs");
            foreach (var item in media.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String || item.GetString() is not { } reference ||
                    reference.Length > 2048 || !Uri.TryCreate(reference, UriKind.Absolute, out var uri) ||
                    uri.Scheme is not ("http" or "https"))
                    throw ImportInputError(row, "lesson.activities.config.media_refs");
            }
        }
        catch (JsonException) { throw ImportInputError(row, "lesson.activities.config"); }
    }

    private static DomainRuleException ImportInputError(CurriculumImportRow? row, string field)
    {
        var error = new DomainRuleException(ErrorCodes.InvalidInput, "Invalid curriculum import input.");
        error.Data["details"] = new
        {
            source_position = row?.SourcePosition,
            source_row_key = row?.SourceRowKey,
            field
        };
        return error;
    }

    private sealed record RowHashPayload(
        [property: JsonPropertyName("source_ref")] string SourceRef,
        [property: JsonPropertyName("source_row_key")] string SourceRowKey,
        [property: JsonPropertyName("source_approved")] bool? SourceApproved,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("objective")] string Objective,
        [property: JsonPropertyName("instructions")] string Instructions,
        [property: JsonPropertyName("missions")] IReadOnlyList<MissionInput>? Missions,
        [property: JsonPropertyName("activities")] IReadOnlyList<ActivityInput>? Activities);
}
