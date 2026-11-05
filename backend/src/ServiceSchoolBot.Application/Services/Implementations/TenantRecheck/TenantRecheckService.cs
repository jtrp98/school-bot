using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Application.Services.Implementations.TenantRecheck;

public sealed class TenantRecheckService(
    ILicenseRepository schoolBotDb,
    ISchoolProfileRepository schoolDb,
    TimeProvider clock,
    IWebSessionRepository sessions) : ITenantRecheckService
{
    public async Task<TenantRecheckResult> RecheckTenantBeforeCommitAsync(
        int schoolId,
        string profileKind,
        int profileId,
        string? sessionTokenHash = null,
        CancellationToken ct = default)
    {
        if (schoolId <= 0)
            return TenantRecheckResult.Deny(ErrorCodes.InvalidInput, "Invalid school ID.");

        var now = clock.GetUtcNow();

        // 1. Re-check profile on legacy School DB connection
        if (string.Equals(profileKind, "student", StringComparison.OrdinalIgnoreCase))
        {
            var isStudentActive = await schoolDb.IsStudentActiveAsync(schoolId, profileId, ct);
            if (!isStudentActive)
                return TenantRecheckResult.Deny(ErrorCodes.SchoolMismatch, "Student profile is not active in this school.");
        }
        else if (string.Equals(profileKind, "employee", StringComparison.OrdinalIgnoreCase))
        {
            var isEmployeeActive = await schoolDb.IsEmployeeActiveAsync(schoolId, profileId, ct);
            if (!isEmployeeActive)
                return TenantRecheckResult.Deny(ErrorCodes.SchoolMismatch, "Employee profile is not active in this school.");
        }
        else
        {
            return TenantRecheckResult.Deny(ErrorCodes.InvalidInput, "Unsupported profile kind.");
        }

        // 2. Re-check license on SchoolBot DB connection
        var licenses = await schoolBotDb.ListLicensesAsync(schoolId, ct);
        var activeLicense = licenses.FirstOrDefault(l =>
            !l.IsDelete &&
            l.RevokedAt is null &&
            l.ExpiresAt > now);

        if (activeLicense is null)
            return TenantRecheckResult.Deny(ErrorCodes.LicenseInactive, "Active school license is required.");

        // 3. Re-check session on SchoolBot DB connection if supplied
        if (!string.IsNullOrWhiteSpace(sessionTokenHash))
        {
            var session = await sessions.GetSessionByTokenHashAsync(sessionTokenHash, ct);
            if (session is null || session.IsDelete || session.RevokedAt is not null || session.ExpiresAt <= now)
                return TenantRecheckResult.Deny(ErrorCodes.SessionExpired, "Session is revoked or expired.");

            if (session.SchoolId != schoolId)
                return TenantRecheckResult.Deny(ErrorCodes.SchoolMismatch, "Session school does not match operation school.");
        }

        return TenantRecheckResult.Allow();
    }
}
