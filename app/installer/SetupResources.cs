using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace SchoolBot.Installer;

internal static class SetupResources
{
    private static readonly Assembly Assembly = typeof(SetupResources).Assembly;
    private static readonly Dictionary<string, string> Strings = ReadJson<Dictionary<string, string>>("Setup.Strings");
    internal static readonly PayloadInfo Payload = ReadJson<PayloadInfo>("Setup.PayloadInfo");

    internal static string Text(string key) => Strings.TryGetValue(key, out var value)
        ? value
        : throw new InvalidDataException($"Missing installer translation: {key}");

    internal static string Format(string key, string parameter, string value) =>
        Text(key).Replace("{{" + parameter + "}}", value, StringComparison.Ordinal);

    private static T ReadJson<T>(string name)
    {
        using var resource = Open(name);
        return JsonSerializer.Deserialize<T>(resource)
            ?? throw new InvalidDataException($"Invalid installer resource: {name}");
    }

    private static Stream Open(string name) => Assembly.GetManifestResourceStream(name)
        ?? throw new InvalidDataException($"Missing installer resource: {name}");

    internal static void VerifyPayload()
    {
        using var resource = Open("Setup.Package");
        var actual = Convert.ToHexString(SHA256.HashData(resource));
        if (!actual.Equals(Payload.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Installer payload checksum mismatch.");
    }

    internal static string CachePackage()
    {
        VerifyPayload();
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SchoolBot", "InstallerCache", Payload.Sha256);
        Directory.CreateDirectory(directory);
        var package = Path.Combine(directory, "SchoolBot.msi");
        if (!File.Exists(package) || !MatchesHash(package))
        {
            using var source = Open("Setup.Package");
            using var target = new FileStream(package, FileMode.Create, FileAccess.Write, FileShare.None);
            source.CopyTo(target);
        }
        if (!MatchesHash(package))
            throw new InvalidDataException("Cached installer checksum mismatch.");
        using var notices = Open("Setup.Notices");
        using var destination = File.Create(Path.Combine(directory, "THIRD-PARTY-NOTICES.txt"));
        notices.CopyTo(destination);
        return package;
    }

    private static bool MatchesHash(string path)
    {
        using var file = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(file)).Equals(Payload.Sha256, StringComparison.OrdinalIgnoreCase);
    }
}

internal sealed record PayloadInfo(string Version, string Sha256);
