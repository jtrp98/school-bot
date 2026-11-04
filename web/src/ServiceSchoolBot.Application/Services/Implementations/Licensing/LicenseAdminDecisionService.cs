using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Application.Services.Implementations.Licensing;

public static class LicenseAdminDecisionService
{
    private const int MaxReplayWindowSeconds = 900;
    private const string LicenseAdminRole = "license_admin";
    private const string StaffAdminChannel = "staff_admin";

    public static LicenseAdminAuthorizationResult EvaluateAuthorization(
        LicenseAdminAuthContext context,
        DateTimeOffset serverNow)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.IsAuthConfigured)
            return LicenseAdminAuthorizationResult.Denied(
                503,
                ErrorCodes.AuthNotConfigured,
                "The staff authentication system is not configured.");

        if (string.IsNullOrWhiteSpace(context.Token) || string.IsNullOrWhiteSpace(context.Principal))
            return LicenseAdminAuthorizationResult.Denied(
                401,
                ErrorCodes.Unauthorized,
                "A valid staff authentication token is required.");

        if (context.IsSessionExpired)
            return LicenseAdminAuthorizationResult.Denied(
                401,
                ErrorCodes.SessionExpired,
                "The staff session has expired.");

        if (!string.Equals(context.Channel, StaffAdminChannel, StringComparison.Ordinal))
            return LicenseAdminAuthorizationResult.Denied(
                403,
                ErrorCodes.ChannelNotAllowed,
                "The staff_admin channel is required.");

        var hasRole = context.Roles is not null &&
            context.Roles.Any(grant => grant.IsActive(serverNow, LicenseAdminRole));

        if (!hasRole)
            return LicenseAdminAuthorizationResult.Denied(
                403,
                ErrorCodes.StaffRoleRequired,
                "The active license_admin role is required.");

        return LicenseAdminAuthorizationResult.Allowed();
    }

    public static LicenseAdminReplayDecision EvaluateReplay(
        LicenseAdminAuthContext authContext,
        long schoolId,
        string principal,
        string operation,
        Guid requestKey,
        string requestHash,
        DateTimeOffset serverNow,
        LicenseAdminReplayRecord? storedRecord,
        LicenseCurrentState? currentLicense,
        Func<byte[], string, string?>? decryptCipherSeam)
    {
        var auth = EvaluateAuthorization(authContext, serverNow);
        if (!auth.IsAllowed)
            return LicenseAdminReplayDecision.Denied(auth.StatusCode, auth.ErrorCode!, auth.Message!);

        if (storedRecord is null)
            return LicenseAdminReplayDecision.ProceedNew();

        var ceiling = storedRecord.CompletedAt.AddSeconds(MaxReplayWindowSeconds);
        var effectiveReplayUntil = storedRecord.ReplayUntil <= ceiling
            ? storedRecord.ReplayUntil
            : ceiling;

        if (serverNow >= effectiveReplayUntil)
            return LicenseAdminReplayDecision.Expired(
                storedRecord.ResourceId,
                LicenseAdminCleanupAction.ClearCipher(storedRecord.ResourceId));

        if (storedRecord.SchoolId != schoolId ||
            !string.Equals(storedRecord.Principal, principal, StringComparison.Ordinal) ||
            !string.Equals(storedRecord.Operation, operation, StringComparison.Ordinal) ||
            storedRecord.RequestKey != requestKey ||
            !string.Equals(storedRecord.RequestHash, requestHash, StringComparison.Ordinal))
        {
            return LicenseAdminReplayDecision.Conflict(
                ErrorCodes.IdempotencyConflict,
                "Idempotency key was previously used with different parameters.");
        }

        if (currentLicense is null)
            return LicenseAdminReplayDecision.Conflict(
                ErrorCodes.LicenseInvalid,
                "Target license was not found.");

        if (currentLicense.Revoked)
            return LicenseAdminReplayDecision.Conflict(
                ErrorCodes.LicenseInactive,
                "Target license has been revoked.",
                storedRecord.ResourceId);

        if (storedRecord.ResultCipher is null || storedRecord.ResultKeyId is null)
            return LicenseAdminReplayDecision.Expired(
                storedRecord.ResourceId,
                LicenseAdminCleanupAction.None);

        if (decryptCipherSeam is null)
            return LicenseAdminReplayDecision.Conflict(
                ErrorCodes.StateConflict,
                "Decryption seam is not configured.",
                storedRecord.ResourceId);

        string? plainKey;
        try
        {
            plainKey = decryptCipherSeam(storedRecord.ResultCipher, storedRecord.ResultKeyId);
        }
        catch (Exception)
        {
            return LicenseAdminReplayDecision.Conflict(
                ErrorCodes.StateConflict,
                "Failed to decrypt replayed license key.",
                storedRecord.ResourceId);
        }

        if (string.IsNullOrEmpty(plainKey))
            return LicenseAdminReplayDecision.Conflict(
                ErrorCodes.StateConflict,
                "Failed to decrypt replayed license key.",
                storedRecord.ResourceId);

        return LicenseAdminReplayDecision.Replayed(storedRecord.ResourceId, plainKey);
    }
}
