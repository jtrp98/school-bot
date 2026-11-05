using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ServiceSchoolBot.Infrastructure.Authentication;

internal static class GatewayFieldValidation
{
    private static readonly Regex TimestampPattern = new(
        @"\A[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-5][0-9](?:\.[0-9]{1,7})?(?:Z|[+-][0-9]{2}:[0-9]{2})\z",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    internal static bool HasAny(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out _)) return true;
        }
        return false;
    }

    internal static bool HasMatchingCode(JsonElement root, int httpStatus)
    {
        if (!root.TryGetProperty("code", out var code)) return httpStatus != 200;
        var text = code.ValueKind switch
        {
            JsonValueKind.String => code.GetString(),
            JsonValueKind.Number => code.GetRawText(),
            _ => null
        };
        return text is { Length: 3 } && text[0] is >= '1' and <= '5' &&
            text[1] is >= '0' and <= '9' && text[2] is >= '0' and <= '9' &&
            (text[0] - '0') * 100 + (text[1] - '0') * 10 + text[2] - '0' == httpStatus;
    }

    internal static bool TrySecret(JsonElement element, string name, out string value, bool challenge = false)
    {
        value = "";
        if (!element.TryGetProperty(name, out var field) || field.ValueKind != JsonValueKind.String) return false;
        var text = field.GetString()!;
        if (text.Length == 0 || (challenge && text.Length > 200) || Encoding.UTF8.GetByteCount(text) > 16_384)
            return false;
        foreach (var character in text)
        {
            if (char.IsWhiteSpace(character) || char.IsControl(character)) return false;
        }
        value = text;
        return true;
    }

    internal static bool TryPresentation(JsonElement element, string name, out string value)
    {
        value = "";
        if (!element.TryGetProperty(name, out var field) || field.ValueKind != JsonValueKind.String) return false;
        var text = field.GetString()!;
        if (text.Length is < 1 or > 512) return false;
        foreach (var character in text)
        {
            if (char.IsControl(character)) return false;
        }
        value = text;
        return true;
    }

    internal static bool TryResendAfterSeconds(JsonElement element, out int seconds)
    {
        seconds = 0;
        if (!element.TryGetProperty("resendAfterSeconds", out var field) || field.ValueKind != JsonValueKind.Number)
            return false;
        var text = field.GetRawText();
        foreach (var character in text)
        {
            if (character is < '0' or > '9') return false;
        }
        return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out seconds);
    }

    internal static bool TryExpiresAt(JsonElement element, out DateTimeOffset expiresAt)
    {
        expiresAt = default;
        if (!element.TryGetProperty("expiresAt", out var field) || field.ValueKind != JsonValueKind.String)
            return false;
        var text = field.GetString()!;
        if (text.Length is < 20 or > 33 || !TimestampPattern.IsMatch(text)) return false;
        if (text.EndsWith('Z')) text = text[..^1] + "+00:00";
        var format = text.Contains('.') ? "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz" : "yyyy-MM-dd'T'HH:mm:sszzz";
        if (!DateTimeOffset.TryParseExact(
            text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return false;
        expiresAt = date.ToUniversalTime();
        return true;
    }
}
