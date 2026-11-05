using ServiceSchoolBot.Application.Common.Model.Projects;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Application.Services.Implementations.Projects;

public static class ProjectAuthorizationService
{
    public static ProjectAuthorizationResult Evaluate(ProjectAuthContext context)
    {
        if (!context.IsAuthConfigured)
            return ProjectAuthorizationResult.Denied(503, ErrorCodes.AuthNotConfigured, "Authentication service is not configured.");

        if (string.IsNullOrWhiteSpace(context.Token) || context.OwnerId is null || string.IsNullOrWhiteSpace(context.OwnerKind))
            return ProjectAuthorizationResult.Denied(401, ErrorCodes.Unauthorized, "Authentication is required.");

        if (context.IsSessionExpired)
            return ProjectAuthorizationResult.Denied(401, ErrorCodes.SessionExpired, "Session is expired or revoked.");

        if (!string.Equals(context.Channel, ProjectPrincipalStub.DesktopChannel, StringComparison.OrdinalIgnoreCase))
            return ProjectAuthorizationResult.Denied(403, ErrorCodes.ChannelNotAllowed, "Channel not allowed for project management.");

        if (!context.HasActiveLicense)
            return ProjectAuthorizationResult.Denied(403, ErrorCodes.LicenseRequired, "Active school license is required.");

        if (!string.Equals(context.OwnerKind, "student", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(context.OwnerKind, "employee", StringComparison.OrdinalIgnoreCase))
            return ProjectAuthorizationResult.Denied(403, ErrorCodes.InvalidInput, "Unsupported profile kind.");

        if (context.SchoolId is null || context.SchoolId <= 0)
            return ProjectAuthorizationResult.Denied(400, ErrorCodes.InvalidInput, "Invalid school ID.");

        return ProjectAuthorizationResult.Allowed();
    }
}
