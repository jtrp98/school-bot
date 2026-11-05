using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using System.Collections.Concurrent;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Domain.Releases;

namespace ServiceSchoolBot.Infrastructure.Persistence;

public sealed class InMemoryManagementStore : IManagementStore
{
    private readonly ConcurrentDictionary<string, SchoolLicense> licenses = new();
    private readonly ConcurrentDictionary<string, string> licenseIdsByKeyHash = new();
    private readonly ConcurrentDictionary<string, DesktopRelease> releases = new();
    private readonly ConcurrentDictionary<string, string> releaseIdsByIdentity = new();

    public IReadOnlyList<SchoolLicense> Licenses() => licenses.Values.ToArray();

    public SchoolLicense? FindLicenseById(string id) => licenses.GetValueOrDefault(id);

    public SchoolLicense? FindLicenseByKeyHash(byte[] keyHash) =>
        licenseIdsByKeyHash.TryGetValue(Convert.ToHexString(keyHash), out var id) ? FindLicenseById(id) : null;

    public void Save(SchoolLicense license)
    {
        licenses[license.Id] = license;
        licenseIdsByKeyHash[Convert.ToHexString(license.KeyHash)] = license.Id;
    }

    public IReadOnlyList<DesktopRelease> Releases() => releases.Values.ToArray();

    public DesktopRelease? FindReleaseById(string id) => releases.GetValueOrDefault(id);

    public bool ReleaseExists(string version, string channel, string platform) =>
        releaseIdsByIdentity.ContainsKey(Identity(version, channel, platform));

    public IReadOnlyList<DesktopRelease> PublishedReleases(string channel, string platform) => releases.Values
        .Where(value => value.Published && value.Channel == channel && value.Platform == platform)
        .ToArray();

    public void Save(DesktopRelease release)
    {
        releases[release.Id] = release;
        releaseIdsByIdentity[Identity(release.Version, release.Channel, release.Platform)] = release.Id;
    }

    private static string Identity(string version, string channel, string platform) =>
        $"{version}|{channel}|{platform}";
}
