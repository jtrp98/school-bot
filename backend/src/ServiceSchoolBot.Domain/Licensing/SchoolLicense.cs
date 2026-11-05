namespace ServiceSchoolBot.Domain.Licensing;

// Immutable state: repositories cannot accidentally change an activated license in place.
public sealed record SchoolLicense(
    string Id, string School, string KeyHint, byte[] KeyHash,
    int DeviceLimit, DateTimeOffset ExpiresAt, bool Revoked, IReadOnlyList<string> Devices)
{
    public static SchoolLicense Create(string school, int limit, DateTimeOffset expiresAt,
        string hint, byte[] keyHash, DateTimeOffset now)
    {
        school = school.Trim();
        if (school.Length is < 1 or > 200 || limit is < 1 or > 1000 || expiresAt <= now)
            throw new DomainRuleException(ErrorCodes.InvalidLicense,
                "School, future expiry, and device limit 1–1000 are required.");
        // UTC keeps responses consistent with the "Z" timestamps in docs/API.md whatever offset the caller used.
        return new(Guid.NewGuid().ToString("N"), school, hint, keyHash, limit,
            expiresAt.ToUniversalTime(), false, []);
    }

    public SchoolLicense Activate(string deviceId, DateTimeOffset now)
    {
        EnsureActive(now);
        if (Devices.Contains(deviceId)) return this;
        if (Devices.Count >= DeviceLimit)
            throw new DomainRuleException(ErrorCodes.DeviceLimit, "License device limit reached.");
        return this with { Devices = [.. Devices, deviceId] };
    }

    public void Validate(string deviceId, DateTimeOffset now)
    {
        EnsureActive(now);
        if (!Devices.Contains(deviceId))
            throw new DomainRuleException(ErrorCodes.DeviceNotActivated, "Activate this device first.");
    }

    private void EnsureActive(DateTimeOffset now)
    {
        if (Revoked || ExpiresAt <= now)
            throw new DomainRuleException(ErrorCodes.LicenseInactive, "License expired or revoked.");
    }
}
