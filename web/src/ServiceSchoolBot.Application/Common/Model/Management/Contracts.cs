namespace ServiceSchoolBot.Application.Common.Model.Management;

public sealed record CreateLicenseRequest(string School, int DeviceLimit, DateTimeOffset ExpiresAt);
public sealed record LicenseRequest(string Key, string DeviceId);
public sealed record LicenseView(string Id, string School, string KeyHint, int DeviceLimit,
    int ActivatedDevices, DateTimeOffset ExpiresAt, bool Revoked);
public sealed record CreatedLicense(LicenseView License, string Key, string Notice);
public sealed record ActivationResult(bool Valid, string LicenseId, string School,
    DateTimeOffset ExpiresAt, bool OfflineEntitlement = false);
public sealed record CreateReleaseRequest(string Version, string Channel, string Platform,
    string Url, string Sha256, string Signature);
