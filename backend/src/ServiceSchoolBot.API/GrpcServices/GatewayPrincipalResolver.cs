using System.Globalization;
using System.Text.Json;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Common.Model.Projects;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.API.GrpcServices;

// Turns the Gateway identity into the principal contexts the services already authorize with:
// central staff (TAdmin) get every staff role on the staff_admin channel, a verified device entitlement
// makes the caller a desktop user, other employees use the school web channel.
public sealed class GatewayPrincipalResolver(
    IMasterAccountRepository master,
    ISchoolProfileRepository school,
    ILicenseRepository licenses,
    ILicenseTokenSigner signer,
    TimeProvider clock) : IProjectAuthResolver, ICurriculumAuthResolver, ILicenseAdminAuthResolver
{
    private const string GatewayToken = "gateway";
    private const string DesktopChannel = "desktop";
    private const string SchoolWebChannel = "school_web";
    private const string StaffAdminChannel = "staff_admin";
    private static readonly string[] StaffRoles = ["content_author", "content_approver", "license_admin"];

    public async Task<ProjectAuthContext> ResolveAsync(GatewayIdentity? identity, CancellationToken ct = default)
    {
        if (identity is null) return new ProjectAuthContext(true, null, false, null, null, null, null);
        var desktop = await IsDesktopAsync(identity, licenses, ct);
        var channel = identity.AdminId is not null ? StaffAdminChannel : desktop ? DesktopChannel : SchoolWebChannel;
        var now = clock.GetUtcNow();
        var licensed = (await licenses.ListLicensesAsync(identity.SchoolId, ct))
            .Any(license => !license.IsDelete && license.RevokedAt is null && license.ExpiresAt > now);
        return new ProjectAuthContext(true, GatewayToken, false, channel, identity.SchoolId, identity.UserType,
            identity.UserType is null ? null : identity.ProfileId, identity.UserId,
            licensed && (identity.DeviceEntitlement is null || desktop));
    }

    async Task<CurriculumPrincipalContext> ICurriculumAuthResolver.ResolveAsync(GatewayIdentity? identity, CancellationToken ct)
    {
        try
        {
            return identity is null ? Anonymous : await ResolveCurriculumAsync(identity, licenses, ct);
        }
        catch (DomainRuleException) { throw; }
        catch (Exception) when (!ct.IsCancellationRequested)
        {
            throw new DomainRuleException(ErrorCodes.ServiceUnavailable, "Authentication is unavailable.");
        }
    }

    public LicenseAdminAuthContext Resolve(GatewayIdentity? identity) =>
        identity?.AdminId is { } adminId
            ? new LicenseAdminAuthContext(true, $"admin:{adminId}", GatewayToken, false, StaffAdminChannel,
                [new StaffRoleGrant("license_admin", DateTimeOffset.MinValue)])
            : new LicenseAdminAuthContext(true, null, null, false, null, null);

    private static CurriculumPrincipalContext Anonymous => new(true, null, null, false, null);

    // Re-resolved inside the commit transaction (RecheckAsync) so the authorization check and the mutation
    // see the same license and binding state.
    private async Task<CurriculumPrincipalContext> ResolveCurriculumAsync(
        GatewayIdentity identity, ILicenseRepository licenseRepository, CancellationToken ct)
    {
        if (identity.AdminId is { } adminId)
            return new CurriculumPrincipalContext(true, $"admin:{adminId}", GatewayToken, false, StaffAdminChannel,
                Roles: StaffRoles.Select(role => new CurriculumStaffRoleGrant(role, DateTimeOffset.MinValue)).ToArray());

        if (!await master.IsSchoolActiveAsync(identity.SchoolId, ct)) return Anonymous;
        var profileId = identity.ProfileId;
        if (identity.DeviceEntitlement is not null)
        {
            if (!await IsDesktopAsync(identity, licenseRepository, ct))
                throw new DomainRuleException(ErrorCodes.LicenseRequired, "An active device license is required.");
            var active = identity.UserType switch
            {
                GatewayIdentity.Student => await school.IsStudentActiveAsync(identity.SchoolId, profileId, ct),
                GatewayIdentity.Employee => await school.IsEmployeeActiveAsync(identity.SchoolId, profileId, ct),
                _ => false
            };
            return active
                ? new CurriculumPrincipalContext(true,
                    $"{identity.SchoolId}:desktop:{identity.UserType}:{profileId}", GatewayToken, false, DesktopChannel,
                    identity.SchoolId, false, true, RecheckAsync: Recheck(identity))
                : Anonymous;
        }

        if (identity.UserType != GatewayIdentity.Employee) return Anonymous;
        var employee = await school.FindEmployeeProfileAsync(identity.SchoolId, profileId, ct);
        if (employee is null || !employee.IsActive || employee.SchoolId != identity.SchoolId) return Anonymous;
        var now = clock.GetUtcNow();
        var licensed = (await licenseRepository.ListLicensesAsync(identity.SchoolId, ct))
            .Any(license => !license.IsDelete && license.RevokedAt is null && license.ExpiresAt > now);
        if (!licensed)
            throw new DomainRuleException(ErrorCodes.LicenseRequired, "An active school license is required.");
        return new CurriculumPrincipalContext(true,
            $"{identity.SchoolId}:{profileId.ToString(CultureInfo.InvariantCulture)}", GatewayToken, false,
            SchoolWebChannel, identity.SchoolId, identity.IsSchoolAdmin || employee.IsAdmin, true, [],
            Recheck(identity));
    }

    private Func<ISchoolBotRepositoryScope, CancellationToken, Task<CurriculumPrincipalContext>> Recheck(
        GatewayIdentity identity) =>
        (transaction, cancellation) => ResolveCurriculumAsync(identity, transaction, cancellation);

    // The entitlement must be ours, unexpired, for this school, and still match the license's current binding.
    private async Task<bool> IsDesktopAsync(GatewayIdentity identity, ILicenseRepository repository, CancellationToken ct)
    {
        if (identity.DeviceEntitlement is not { } token ||
            !signer.Verify(token, out var header, out var payload) || header is null || payload is null)
            return false;
        using (header)
        using (payload)
        {
            var claims = payload.RootElement;
            if (!header.RootElement.TryGetProperty("typ", out var typ) || typ.GetString() != "schoolbot-entitlement+jwt" ||
                !Text(claims, "aud", "schoolbot-desktop") ||
                !Number(claims, "exp", out var exp) || DateTimeOffset.FromUnixTimeSeconds(exp) <= clock.GetUtcNow() ||
                !Number(claims, "school_id", out var schoolId) || schoolId != identity.SchoolId ||
                !Number(claims, "license_id", out var licenseId) ||
                !Number(claims, "binding_id", out var bindingId) ||
                !Number(claims, "binding_epoch", out var epoch) ||
                !claims.TryGetProperty("cnf", out var cnf) || !cnf.TryGetProperty("jkt", out var jktElement) ||
                jktElement.GetString() is not { Length: > 0 } jkt)
                return false;

            var now = clock.GetUtcNow();
            var license = (await repository.ListLicensesAsync(identity.SchoolId, ct))
                .FirstOrDefault(row => row.SchoolBotLicenseId == licenseId);
            if (license is null || license.IsDelete || license.RevokedAt is not null || license.ExpiresAt <= now ||
                license.CurrentBindingId != bindingId || license.BindingEpoch != epoch)
                return false;
            var binding = await repository.GetBindingByIdAsync((int)bindingId, ct);
            return binding is not null && !binding.IsDelete && binding.DetachedAt is null &&
                binding.SchoolId == identity.SchoolId && binding.LicenseId == license.SchoolBotLicenseId &&
                binding.Epoch == license.BindingEpoch && binding.DeviceJkt == jkt;
        }
    }

    private static bool Text(JsonElement claims, string name, string expected) =>
        claims.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String &&
        value.GetString() == expected;

    private static bool Number(JsonElement claims, string name, out long value)
    {
        value = 0;
        return claims.TryGetProperty(name, out var element) && element.ValueKind == JsonValueKind.Number &&
            element.TryGetInt64(out value);
    }
}
