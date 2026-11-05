using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ServiceSchoolBot.Domain.Assessment;

public static class LegacyScoreDocument
{
    public static JsonObject Parse(string document)
    {
        if (System.Text.Encoding.UTF8.GetByteCount(document) > 2 * 1024 * 1024) throw Invalid();
        using var json = JsonDocument.Parse(document, new JsonDocumentOptions { MaxDepth = 32 });
        Unique(json.RootElement);
        return JsonNode.Parse(document)?.AsObject() ?? throw Invalid();
    }

    public static JsonArray Scores(JsonObject document)
    {
        var node = document["ScoreData"];
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            using var parsed = JsonDocument.Parse(text, new JsonDocumentOptions { MaxDepth = 32 });
            Unique(parsed.RootElement);
            node = JsonNode.Parse(text);
        }
        if (node is not JsonArray array || array.Count > 1000) throw Invalid();
        foreach (var entry in array)
            if (entry is not JsonObject || entry["AssessmentId"] is not JsonValue id || !id.TryGetValue<int>(out var number) || number <= 0 ||
                entry["ScoreIdentifier"] is not JsonValue identifier || !identifier.TryGetValue<string>(out var key) || string.IsNullOrWhiteSpace(key)) throw Invalid();
        if (array.Select(e => e!["AssessmentId"]!.GetValue<int>()).Distinct().Count() != array.Count ||
            array.Select(e => e!["ScoreIdentifier"]!.GetValue<string>()).Distinct(StringComparer.Ordinal).Count() != array.Count) throw Invalid();
        return array;
    }

    public static JsonObject Cell(JsonArray scores, int assessmentId, string identifier)
    {
        var matches = scores.Where(e => e!["AssessmentId"]!.GetValue<int>() == assessmentId ||
            e["ScoreIdentifier"]!.GetValue<string>() == identifier).ToArray();
        if (matches.Length != 1 || matches[0]!["AssessmentId"]!.GetValue<int>() != assessmentId ||
            matches[0]!["ScoreIdentifier"]!.GetValue<string>() != identifier) throw Invalid();
        return matches[0]!.AsObject();
    }

    public static decimal? Value(JsonObject cell)
    {
        if (cell["Score"] is null) return null;
        if (cell["Score"] is not JsonValue value || !value.TryGetValue<string>(out var text)) throw Invalid();
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (text.Length > 64 || !decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var score) || score < 0) throw Invalid();
        return score;
    }
    private static void Unique(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array) foreach (var item in element.EnumerateArray()) Unique(item);
        if (element.ValueKind != JsonValueKind.Object) return;
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject()) { if (!names.Add(property.Name)) throw Invalid(); Unique(property.Value); }
    }
    private static DomainRuleException Invalid() => new("TARGET_NOT_READY", "Existing grade document or cell identifiers are ambiguous.");
}
