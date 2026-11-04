using System.Text.RegularExpressions;

namespace ServiceSchoolBot.Domain.Releases;

public sealed record DesktopRelease(string Id, string Version, string Channel, string Platform,
    string Url, string Sha256, string Signature, bool Published)
{
    public const int MaxUrlLength = 2048;
    public const int MaxSignatureLength = 4096;

    // \z, not $: "$" also matches before a trailing newline, which would let "1.2.3\n" past the
    // duplicate check as a different string.
    private static readonly Regex VersionPattern =
        new(@"^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\z", RegexOptions.CultureInvariant);

    public static DesktopRelease Create(string version, string channel, string platform,
        string url, string sha256, string signature)
    {
        if (!ValidVersion(version) || !ValidChannel(channel) || !ValidPlatform(platform))
            throw new DomainRuleException(ErrorCodes.InvalidRelease,
                "Use x.y.z, stable/beta, and a supported OS/architecture.");
        if (!ValidArtifact(url, sha256, signature))
            throw new DomainRuleException(ErrorCodes.InvalidArtifact,
                "HTTPS URL, SHA256 and signature metadata are required. PoC does not verify signatures.");
        return new(Guid.NewGuid().ToString("N"), version, channel, platform, url,
            sha256.ToLowerInvariant(), signature, false);
    }

    public static bool ValidChannel(string value) => value is "stable" or "beta";
    public static bool ValidPlatform(string value) => value is "windows-amd64" or "darwin-amd64" or "darwin-arm64";
    public static bool ValidVersion(string value) =>
        VersionPattern.IsMatch(value) && System.Version.TryParse(value, out _);
    public static bool Newer(string a, string b) => System.Version.Parse(a) > System.Version.Parse(b);

    private static bool ValidArtifact(string url, string sha256, string signature)
    {
        var validUrl = url.Length <= MaxUrlLength && Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            uri.Scheme == "https" && !string.IsNullOrEmpty(uri.Host) && uri.UserInfo.Length == 0;
        var validHash = sha256.Length == 64 && sha256.All(Uri.IsHexDigit);
        var validSignature = !string.IsNullOrWhiteSpace(signature) && signature.Length <= MaxSignatureLength;
        return validUrl && validHash && validSignature;
    }
}
