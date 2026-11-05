using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace ServiceSchoolBot.Infrastructure.Authentication;

internal static class GatewayJsonValidation
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly JsonReaderOptions ReaderOptions = new()
    {
        MaxDepth = 32,
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow
    };

    private static readonly string[] RootNames = ["code", "results", "message", "messageTh", "requestId"];
    private static readonly string[] ResultNames =
    [
        "requirePasswordChange", "changeToken", "next", "challengeToken", "expiresAt", "token",
        "trustedDeviceToken", "ref", "masked", "resendAfterSeconds", "channels", "missing", "user",
        "retryAfterSeconds", "temporaryToken", "isDeviceVerified", "forceResetPassword"
    ];

    internal static bool IsWhitespace(ReadOnlySpan<byte> json)
    {
        foreach (var value in json)
        {
            if (value is not ((byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n')) return false;
        }
        return true;
    }

    internal static bool TryParse(ReadOnlySpan<byte> json, [NotNullWhen(true)] out JsonDocument? document)
    {
        document = null;
        try
        {
            StrictUtf8.GetCharCount(json);
            var reader = new Utf8JsonReader(json, ReaderOptions);
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) return false;
            var containers = new Stack<HashSet<string>?>();
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        containers.Push(new HashSet<string>(StringComparer.Ordinal));
                        break;
                    case JsonTokenType.StartArray:
                        containers.Push(null);
                        break;
                    case JsonTokenType.EndObject:
                    case JsonTokenType.EndArray:
                        containers.Pop();
                        break;
                    case JsonTokenType.PropertyName:
                        if (!HasValidSurrogates(reader.ValueSpan)) return false;
                        if (!containers.Peek()!.Add(reader.GetString()!)) return false;
                        break;
                    case JsonTokenType.String:
                        if (!HasValidSurrogates(reader.ValueSpan)) return false;
                        break;
                }
            } while (reader.Read());

            var documentReader = new Utf8JsonReader(json, ReaderOptions);
            document = JsonDocument.ParseValue(ref documentReader);
            if (!HasCorrectCase(document.RootElement, RootNames) ||
                (document.RootElement.TryGetProperty("results", out var results) &&
                 results.ValueKind == JsonValueKind.Object && !HasCorrectCase(results, ResultNames)))
            {
                document.Dispose();
                document = null;
                return false;
            }
            return true;
        }
        catch (Exception error) when (error is JsonException or DecoderFallbackException or InvalidOperationException)
        {
            document?.Dispose();
            document = null;
            return false;
        }
    }

    private static bool HasCorrectCase(JsonElement element, string[] knownNames)
    {
        foreach (var property in element.EnumerateObject())
        {
            foreach (var name in knownNames)
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase) && property.Name != name)
                    return false;
            }
        }
        return true;
    }

    private static bool HasValidSurrogates(ReadOnlySpan<byte> value)
    {
        // JSON readers may defer Unicode unescaping; validate even discarded string values.
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] != (byte)'\\') continue;
            index++;
            if (value[index] != (byte)'u') continue;
            var codeUnit = ReadCodeUnit(value.Slice(index + 1, 4));
            index += 4;
            if (codeUnit is >= 0xDC00 and <= 0xDFFF) return false;
            if (codeUnit is not (>= 0xD800 and <= 0xDBFF)) continue;
            if (index + 6 >= value.Length || value[index + 1] != (byte)'\\' || value[index + 2] != (byte)'u')
                return false;
            var lowSurrogate = ReadCodeUnit(value.Slice(index + 3, 4));
            if (lowSurrogate is not (>= 0xDC00 and <= 0xDFFF)) return false;
            index += 6;
        }
        return true;
    }

    private static int ReadCodeUnit(ReadOnlySpan<byte> digits)
    {
        var result = 0;
        foreach (var digit in digits)
        {
            var number = digit is >= (byte)'0' and <= (byte)'9' ? digit - '0' :
                digit is >= (byte)'a' and <= (byte)'f' ? digit - 'a' + 10 : digit - 'A' + 10;
            result = result * 16 + number;
        }
        return result;
    }
}
