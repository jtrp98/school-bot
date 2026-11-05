using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceSchoolBot.Application.Services.Implementations.Curriculum;

public sealed record LessonImportSource(
    [property: JsonPropertyName("source_ref")] string SourceRef,
    [property: JsonPropertyName("source_row_key")] string SourceRowKey,
    [property: JsonPropertyName("source_approved")] bool? SourceApproved,
    [property: JsonPropertyName("row_hash")] string RowHash);

public static class LessonImportSourceCodec
{
    public static string Encode(LessonImportSource source, Guid lessonKey)
    {
        var json = JsonSerializer.Serialize(source);
        _ = Decode(json, lessonKey);
        return json;
    }

    public static LessonImportSource Decode(string json, Guid lessonKey)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException("Invalid lesson import source.");
        var properties = root.EnumerateObject().ToArray();
        if (properties.Length != 4 ||
            properties.Select(property => property.Name).Distinct(StringComparer.Ordinal).Count() != 4 ||
            !root.TryGetProperty("source_ref", out var sourceRef) || sourceRef.ValueKind != JsonValueKind.String ||
            !root.TryGetProperty("source_row_key", out var sourceRowKey) || sourceRowKey.ValueKind != JsonValueKind.String ||
            !root.TryGetProperty("source_approved", out var approved) ||
            approved.ValueKind is not (JsonValueKind.True or JsonValueKind.False or JsonValueKind.Null) ||
            !root.TryGetProperty("row_hash", out var rowHash) || rowHash.ValueKind != JsonValueKind.String)
            throw new JsonException("Invalid lesson import source.");
        var reference = sourceRef.GetString();
        var key = sourceRowKey.GetString();
        var hash = rowHash.GetString();
        if (string.IsNullOrWhiteSpace(reference) || reference.Length > 2048 ||
            key != lessonKey.ToString("D") || lessonKey == Guid.Empty ||
            hash is null || hash.Length != 64 ||
            hash.Any(character => character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f')))
            throw new JsonException("Invalid lesson import source.");
        return new LessonImportSource(reference, key, approved.ValueKind == JsonValueKind.Null ? null : approved.GetBoolean(), hash);
    }
}
