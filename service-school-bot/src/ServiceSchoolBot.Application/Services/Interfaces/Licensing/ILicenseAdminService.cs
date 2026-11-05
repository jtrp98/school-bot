using ServiceSchoolBot.Application.Common.Model.Licensing;
namespace ServiceSchoolBot.Application.Services.Interfaces.Licensing;

public sealed record LicenseViewV2(
    int Id,
    int SchoolId,
    string SchoolName,
    string KeyHint,
    DateTimeOffset ExpiresAt,
    bool Revoked,
    int ActivatedDevices,
    DateTimeOffset? UpdateDate);

public sealed record CreateLicenseV2Request(
    int SchoolId,
    string SchoolName,
    DateTimeOffset ExpiresAt,
    int? DeviceLimit = null);

public sealed record CreatedLicenseV2Response(
    LicenseViewV2 License,
    string Key);

public sealed record RevokeLicenseV2Request(
    string? Reason = null);

public sealed record UnbindLicenseV2Request(
    string? Reason = null,
    string? NextDeviceJkt = null);

public interface ILicenseAdminService
{
    Task<IReadOnlyList<LicenseViewV2>> ListLicensesAsync(
        LicenseAdminAuthContext authContext,
        int? schoolId = null,
        CancellationToken ct = default);

    Task<CreatedLicenseV2Response> CreateLicenseAsync(
        LicenseAdminAuthContext authContext,
        CreateLicenseV2Request request,
        Guid? requestKey = null,
        CancellationToken ct = default);

    Task<LicenseViewV2> RevokeLicenseAsync(
        LicenseAdminAuthContext authContext,
        int licenseId,
        string? expectedETag = null,
        string? reason = null,
        CancellationToken ct = default);

    Task<LicenseViewV2> UnbindLicenseAsync(
        LicenseAdminAuthContext authContext,
        int licenseId,
        string? expectedETag = null,
        string? nextDeviceJkt = null,
        string? reason = null,
        CancellationToken ct = default);
}
