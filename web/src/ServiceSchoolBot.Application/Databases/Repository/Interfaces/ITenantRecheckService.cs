namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public sealed record TenantRecheckResult(bool Allowed, string? ErrorCode = null, string? Reason = null)
{
    public static TenantRecheckResult Allow() => new(true);
    public static TenantRecheckResult Deny(string errorCode, string reason) => new(false, errorCode, reason);
}

public interface ITenantRecheckService
{
    Task<TenantRecheckResult> RecheckTenantBeforeCommitAsync(
        int schoolId,
        string profileKind,
        int profileId,
        string? sessionTokenHash = null,
        CancellationToken ct = default);
}
