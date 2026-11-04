namespace ServiceSchoolBot.Application.Common.Model.Licensing;

public sealed record StaffRoleGrant(
    string Role,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidUntil = null,
    bool IsDelete = false
)
{
    public bool IsActive(DateTimeOffset at, string requiredRole) =>
        !IsDelete &&
        string.Equals(Role, requiredRole, StringComparison.Ordinal) &&
        at >= ValidFrom &&
        (ValidUntil is null || at < ValidUntil.Value);
}

public sealed record LicenseAdminAuthContext(
    bool IsAuthConfigured,
    string? Principal,
    string? Token,
    bool IsSessionExpired,
    string? Channel,
    IReadOnlyList<StaffRoleGrant>? Roles
);

public sealed record LicenseAdminAuthorizationResult(
    bool IsAllowed,
    int StatusCode,
    string? ErrorCode = null,
    string? Message = null
)
{
    public static LicenseAdminAuthorizationResult Allowed() =>
        new(true, 200);

    public static LicenseAdminAuthorizationResult Denied(int status, string code, string message) =>
        new(false, status, code, message);
}

public sealed record LicenseAdminReplayRecord(
    long SchoolId,
    string Principal,
    string Operation,
    Guid RequestKey,
    string RequestHash,
    DateTimeOffset CompletedAt,
    DateTimeOffset ReplayUntil,
    long ResourceId,
    byte[]? ResultCipher,
    string? ResultKeyId
);

public sealed record LicenseCurrentState(
    long LicenseId,
    bool Revoked
);

public enum LicenseAdminCleanupType
{
    None,
    ClearCipherAndKeyId
}

public sealed record LicenseAdminCleanupAction(
    LicenseAdminCleanupType Type,
    long? ResourceId = null
)
{
    public static readonly LicenseAdminCleanupAction None = new(LicenseAdminCleanupType.None);

    public static LicenseAdminCleanupAction ClearCipher(long resourceId) =>
        new(LicenseAdminCleanupType.ClearCipherAndKeyId, resourceId);
}

public enum LicenseAdminReplayStatus
{
    ProceedNew,
    Replayed,
    Expired,
    Conflict,
    Denied
}

public sealed record LicenseAdminReplayDecision(
    LicenseAdminReplayStatus Status,
    int StatusCode,
    string? ErrorCode,
    string? Message,
    long? ResourceId,
    string? ReplayedKey,
    LicenseAdminCleanupAction CleanupAction
)
{
    public bool IsSuccess => Status is LicenseAdminReplayStatus.ProceedNew or LicenseAdminReplayStatus.Replayed;

    public static LicenseAdminReplayDecision ProceedNew() =>
        new(LicenseAdminReplayStatus.ProceedNew, 200, null, null, null, null, LicenseAdminCleanupAction.None);

    public static LicenseAdminReplayDecision Replayed(long resourceId, string key) =>
        new(LicenseAdminReplayStatus.Replayed, 200, null, null, resourceId, key, LicenseAdminCleanupAction.None);

    public static LicenseAdminReplayDecision Expired(long resourceId, LicenseAdminCleanupAction cleanup) =>
        new(LicenseAdminReplayStatus.Expired, 409, Domain.ErrorCodes.IdempotencyExpired,
            "The idempotency replay window for this license key has expired.", resourceId, null, cleanup);

    public static LicenseAdminReplayDecision Conflict(string errorCode, string message, long? resourceId = null) =>
        new(LicenseAdminReplayStatus.Conflict, 409, errorCode, message, resourceId, null, LicenseAdminCleanupAction.None);

    public static LicenseAdminReplayDecision Denied(int statusCode, string errorCode, string message) =>
        new(LicenseAdminReplayStatus.Denied, statusCode, errorCode, message, null, null, LicenseAdminCleanupAction.None);
}
