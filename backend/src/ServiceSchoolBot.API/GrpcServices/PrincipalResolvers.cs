using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Common.Model.Projects;

namespace ServiceSchoolBot.API.GrpcServices;

// Seams between the Gateway identity and the principal each service authorizes with; tests replace them.

public interface IProjectAuthResolver
{
    Task<ProjectAuthContext> ResolveAsync(GatewayIdentity? identity, CancellationToken ct = default);
}

public interface ICurriculumAuthResolver
{
    Task<CurriculumPrincipalContext> ResolveAsync(GatewayIdentity? identity, CancellationToken ct = default);
}

public interface IAssessmentAuthResolver
{
    Task<AssessmentPrincipalContext> ResolveAsync(GatewayIdentity? identity, CancellationToken ct = default);
}

public interface ILicenseAdminAuthResolver
{
    LicenseAdminAuthContext Resolve(GatewayIdentity? identity);
}

// Assessment authorizes with the curriculum principal plus who the caller is as employee or learner.
public sealed class GatewayAssessmentAuthResolver(ICurriculumAuthResolver curricula) : IAssessmentAuthResolver
{
    public async Task<AssessmentPrincipalContext> ResolveAsync(GatewayIdentity? identity, CancellationToken ct = default)
    {
        var principal = await curricula.ResolveAsync(identity, ct);
        if (identity is null || principal.PrincipalId is null || identity.AdminId is not null) return new(principal);
        return new(principal, identity.UserType == GatewayIdentity.Employee ? identity.ProfileId : null,
            identity.UserType, identity.UserType == GatewayIdentity.Student ? identity.ProfileId : null);
    }
}
