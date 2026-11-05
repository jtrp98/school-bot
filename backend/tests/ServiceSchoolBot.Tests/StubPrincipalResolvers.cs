using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.API.GrpcServices;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Common.Model.Projects;
using ServiceSchoolBot.Application.Services.Implementations.Licensing;

namespace ServiceSchoolBot.Tests;

// Maps each stub credential to the same stub principal the removed bearer-token resolvers produced,
// so the service authorization paths are exercised with unchanged expectations.
public sealed class StubProjectAuthResolver : IProjectAuthResolver
{
    public Task<ProjectAuthContext> ResolveAsync(GatewayIdentity? identity, CancellationToken ct = default) =>
        Task.FromResult(Resolve(StubPrincipals.Find(identity)));

    private static ProjectAuthContext Resolve(StubCredential? credential)
    {
        if (credential?.Has("X-Auth-Unconfigured") == true) return ProjectPrincipalStub.CreateUnconfigured();
        if (string.IsNullOrWhiteSpace(credential?.Token)) return ProjectPrincipalStub.CreateAnonymous();
        var token = credential.Token;
        return token switch
        {
            ProjectPrincipalStub.StudentToken => ProjectPrincipalStub.CreateStudent(),
            ProjectPrincipalStub.Student2Token => ProjectPrincipalStub.CreateStudent2(),
            ProjectPrincipalStub.TeacherToken => ProjectPrincipalStub.CreateTeacher(),
            ProjectPrincipalStub.OtherSchoolStudentToken => ProjectPrincipalStub.CreateOtherSchoolStudent(),
            ProjectPrincipalStub.ExpiredToken => ProjectPrincipalStub.CreateExpired(),
            ProjectPrincipalStub.WrongChannelToken => ProjectPrincipalStub.CreateWrongChannel(),
            ProjectPrincipalStub.NoLicenseToken => ProjectPrincipalStub.CreateNoLicense(),
            ProjectPrincipalStub.UnconfiguredToken => ProjectPrincipalStub.CreateUnconfigured(),
            _ => new ProjectAuthContext(true, token, false, null, null, null, null)
        };
    }
}

public sealed class StubCurriculumAuthResolver(TimeProvider clock) : ICurriculumAuthResolver
{
    public Task<CurriculumPrincipalContext> ResolveAsync(GatewayIdentity? identity, CancellationToken ct = default) =>
        Task.FromResult(Resolve(StubPrincipals.Find(identity)));

    private CurriculumPrincipalContext Resolve(StubCredential? credential)
    {
        if (credential?.Has("X-Auth-Unconfigured") == true)
            return CurriculumPrincipalStub.CreateUnconfigured(CurriculumPrincipalStub.CreateStaffAuthor());
        if (string.IsNullOrWhiteSpace(credential?.Token)) return new(true, null, null, false, null);
        var token = credential.Token;
        if (credential.Has("X-Test-Expired") || string.Equals(token, "token-expired", StringComparison.OrdinalIgnoreCase))
            return CurriculumPrincipalStub.CreateExpired(CurriculumPrincipalStub.CreateStaffAuthor());

        var principal = token switch
        {
            "token-staff-author-valid" => CurriculumPrincipalStub.CreateStaffAuthor(),
            "token-staff-approver-valid" => CurriculumPrincipalStub.CreateStaffApprover(),
            "token-staff-norole-valid" => CurriculumPrincipalStub.CreateStaffNoRole(),
            "token-school-admin-valid" => CurriculumPrincipalStub.CreateSchoolAdmin(),
            "token-school-teacher-valid" => CurriculumPrincipalStub.CreateSchoolTeacher(),
            "token-student-valid" => CurriculumPrincipalStub.CreateDesktopStudent(),
            _ => new CurriculumPrincipalContext(true, token, token, false, CurriculumPrincipalStub.StaffAdminChannel,
                Roles: [new CurriculumStaffRoleGrant(CurriculumPrincipalStub.StaffAuthorRole, clock.GetUtcNow().AddDays(-30))])
        };
        if (credential.Header("X-Test-Principal-Id") is { Length: > 0 } principalId)
            principal = principal with { PrincipalId = principalId };
        if (credential.Header("X-Test-Channel") is { Length: > 0 } channel)
            principal = principal with { Channel = channel };
        if (credential.Header("X-Test-Role") is { Length: > 0 } role)
            principal = principal with { Roles = [new CurriculumStaffRoleGrant(role, clock.GetUtcNow().AddDays(-30))] };
        return principal;
    }
}

public sealed class StubLicenseAdminAuthResolver : ILicenseAdminAuthResolver
{
    public LicenseAdminAuthContext Resolve(GatewayIdentity? identity)
    {
        var credential = StubPrincipals.Find(identity);
        if (credential?.Has("X-Staff-Auth-Unconfigured") == true) return LicenseAdminPrincipalStub.CreateUnconfigured();
        if (string.IsNullOrWhiteSpace(credential?.Token)) return LicenseAdminPrincipalStub.CreateAnonymous();
        var token = credential.Token;
        return token switch
        {
            LicenseAdminPrincipalStub.ValidAdminToken => LicenseAdminPrincipalStub.CreateLicenseAdmin(token: token),
            LicenseAdminPrincipalStub.NoRoleToken => LicenseAdminPrincipalStub.CreateStaffNoRole(token: token),
            LicenseAdminPrincipalStub.OtherRoleToken => LicenseAdminPrincipalStub.CreateStaffOtherRole(token: token),
            LicenseAdminPrincipalStub.WrongChannelToken => LicenseAdminPrincipalStub.CreateWrongChannel(token: token),
            LicenseAdminPrincipalStub.ExpiredToken => LicenseAdminPrincipalStub.CreateExpired(token: token),
            _ => new LicenseAdminAuthContext(true, null, token, false, null, null)
        };
    }
}

// Stub credentials never had a SchoolBot session, so they never resolved to an assessment employee or learner.
public sealed class StubAssessmentAuthResolver(ICurriculumAuthResolver curricula) : IAssessmentAuthResolver
{
    public async Task<AssessmentPrincipalContext> ResolveAsync(GatewayIdentity? identity, CancellationToken ct = default)
    {
        var principal = await curricula.ResolveAsync(identity, ct);
        return principal.Token is null || principal.PrincipalId is null || principal.IsSessionExpired
            ? new(principal)
            : new(principal with { PrincipalId = null });
    }
}

internal static class StubPrincipalRegistration
{
    public static IServiceCollection AddStubPrincipals(this IServiceCollection services)
    {
        services.AddSingleton<IProjectAuthResolver, StubProjectAuthResolver>();
        services.AddSingleton<ICurriculumAuthResolver, StubCurriculumAuthResolver>();
        services.AddSingleton<IAssessmentAuthResolver, StubAssessmentAuthResolver>();
        services.AddSingleton<ILicenseAdminAuthResolver, StubLicenseAdminAuthResolver>();
        return services;
    }
}
