using ServiceSchoolBot.Application.Common.Model.Management;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using System.Security.Cryptography;
using System.Text;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Domain.Releases;

namespace ServiceSchoolBot.Application.Services.Implementations.Management;

// Singleton in the PoC: this lock makes activation limits atomic within one process.
// Multi-instance hosting requires a database transaction/unique constraints instead.
public sealed class ManagementService(IManagementStore store, TimeProvider clock)
{
    private readonly object gate = new();

    private static LicenseView View(SchoolLicense value) => new(value.Id, value.School,
        value.KeyHint, value.DeviceLimit, value.Devices.Count, value.ExpiresAt, value.Revoked);

    public IReadOnlyList<LicenseView> ListLicenses()
    {
        lock (gate) return store.Licenses().Select(View).ToArray();
    }

    public CreatedLicense CreateLicense(CreateLicenseRequest request)
    {
        var key = "SBOT-" + Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
        var license = SchoolLicense.Create(request.School, request.DeviceLimit, request.ExpiresAt,
            key[..8] + "…" + key[^4..], SHA256.HashData(Encoding.UTF8.GetBytes(key)), clock.GetUtcNow());
        lock (gate) store.Save(license);
        return new(View(license), key, "Returned once. Development API; no offline entitlement is issued.");
    }

    public LicenseView RevokeLicense(string id)
    {
        lock (gate)
        {
            var license = store.FindLicenseById(id)
                ?? throw new DomainRuleException(ErrorCodes.NotFound, "License not found.");
            license = license with { Revoked = true };
            store.Save(license);
            return View(license);
        }
    }

    public ActivationResult CheckLicense(LicenseRequest request, bool activate)
    {
        if (string.IsNullOrWhiteSpace(request.Key) || request.Key.Length > 256 ||
            string.IsNullOrWhiteSpace(request.DeviceId) || request.DeviceId.Length is < 8 or > 128)
            throw new DomainRuleException(ErrorCodes.InvalidInput,
                "A key and device ID of 8–128 characters are required.");
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(request.Key));
        lock (gate)
        {
            var license = store.FindLicenseByKeyHash(hash)
                ?? throw new DomainRuleException(ErrorCodes.LicenseInvalid, "License key not found.");
            if (activate)
            {
                license = license.Activate(request.DeviceId, clock.GetUtcNow());
                store.Save(license);
            }
            else
            {
                license.Validate(request.DeviceId, clock.GetUtcNow());
            }
            return new(true, license.Id, license.School, license.ExpiresAt);
        }
    }

    public IReadOnlyList<DesktopRelease> ListReleases()
    {
        lock (gate) return store.Releases();
    }

    public DesktopRelease CreateRelease(CreateReleaseRequest request)
    {
        var release = DesktopRelease.Create(request.Version, request.Channel, request.Platform,
            request.Url, request.Sha256, request.Signature);
        lock (gate)
        {
            if (store.ReleaseExists(release.Version, release.Channel, release.Platform))
                throw new DomainRuleException(ErrorCodes.ReleaseExists, "Version/channel/platform already exists.");
            store.Save(release);
        }
        return release;
    }

    public DesktopRelease PublishRelease(string id)
    {
        lock (gate)
        {
            var release = store.FindReleaseById(id)
                ?? throw new DomainRuleException(ErrorCodes.NotFound, "Release not found.");
            release = release with { Published = true };
            store.Save(release);
            return release;
        }
    }

    public DesktopRelease? CheckUpdate(string version, string channel, string platform)
    {
        if (!DesktopRelease.ValidVersion(version) || !DesktopRelease.ValidChannel(channel) ||
            !DesktopRelease.ValidPlatform(platform))
            throw new DomainRuleException(ErrorCodes.InvalidQuery,
                "Valid version, channel and platform are required.");
        lock (gate)
        {
            return store.PublishedReleases(channel, platform)
                .Where(value => DesktopRelease.Newer(value.Version, version))
                .OrderByDescending(value => System.Version.Parse(value.Version))
                .FirstOrDefault();
        }
    }
}
