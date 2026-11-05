namespace ServiceSchoolBot.Domain.Licensing;

public sealed record SchoolBotLicenseRecord(
    int SchoolBotLicenseId,
    string SchoolName,
    string KeyHash,
    string KeyHint,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    int? CurrentBindingId,
    int BindingEpoch,
    string? AllowedNextJkt,
    bool IsDelete,
    int? CreateBy,
    DateTimeOffset? CreateDate,
    int? UpdateBy,
    DateTimeOffset? UpdateDate,
    int SchoolId);

public sealed record SchoolBotLicenseBindingRecord(
    int SchoolBotLicenseBindingId,
    int LicenseId,
    string DeviceJkt,
    string PublicJwk,
    int Epoch,
    DateTimeOffset ActivatedAt,
    DateTimeOffset? DetachedAt,
    DateTimeOffset LastCheckedAt,
    DateTimeOffset LastEntitlementExpiresAt,
    bool IsDelete,
    int? CreateBy,
    DateTimeOffset? CreateDate,
    int? UpdateBy,
    DateTimeOffset? UpdateDate,
    int SchoolId);

public sealed record CommitLicenseActivationRequest(
    int LicenseId,
    int? ExpectedCurrentBindingId,
    int ExpectedBindingEpoch,
    string DeviceJkt,
    string PublicJwk,
    DateTimeOffset EntitlementExpiresAt);

public sealed record LicenseActivationCommitResult(
    SchoolBotLicenseRecord License,
    SchoolBotLicenseBindingRecord Binding);

public sealed record CommitLicenseUnbindRequest(
    int LicenseId,
    string? ExpectedETag,
    string? NextDeviceJkt,
    int? UpdatedBy);
