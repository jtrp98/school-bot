namespace ServiceSchoolBot.Application.Common.Model.Licensing;

public sealed record DeviceJwk(
    string Kty,
    string Crv,
    string X,
    string Y);

public sealed record CreateChallengeRequest(
    DeviceJwk DeviceJwk);

public sealed record DeviceChallengeResponse(
    string Challenge,
    DateTimeOffset ServerTime,
    DateTimeOffset ExpiresAt);

public sealed record ActivateLicenseRequest(
    string Key,
    DeviceJwk DeviceJwk,
    string Challenge,
    string Proof);

public sealed record ValidateLicenseRequest(
    string Key,
    int BindingId,
    string Challenge,
    string Proof);

public sealed record EntitlementResult(
    bool Valid,
    int LicenseId,
    int BindingId,
    string Entitlement,
    DateTimeOffset CheckedAt,
    DateTimeOffset ExpiresAt);
