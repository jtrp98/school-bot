using System.Globalization;

namespace ServiceSchoolBot.Domain.Concurrency;

public static class OpaqueConcurrencyToken
{
    public static string ToETag(DateTimeOffset? updateDate)
    {
        if (updateDate is null) return string.Empty;
        var ticks = updateDate.Value.ToUniversalTime().Ticks;
        return $"\"{ticks.ToString("x16", CultureInfo.InvariantCulture)}\"";
    }

    public static bool Matches(string? expectedETag, DateTimeOffset? actualUpdateDate)
    {
        if (string.IsNullOrWhiteSpace(expectedETag)) return false;
        var cleanExpected = expectedETag.Trim();
        if (cleanExpected.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
            cleanExpected = cleanExpected[2..].Trim();
        var actual = ToETag(actualUpdateDate);
        return string.Equals(cleanExpected, actual, StringComparison.OrdinalIgnoreCase);
    }
}
