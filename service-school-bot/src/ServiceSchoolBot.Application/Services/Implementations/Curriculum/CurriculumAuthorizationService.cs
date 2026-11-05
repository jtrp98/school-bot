using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Application.Services.Implementations.Curriculum;

public static class CurriculumAuthorizationService
{
    private const string ContentAuthorRole = "content_author";
    private const string ContentApproverRole = "content_approver";
    private const string StaffAdminChannel = "staff_admin";
    private const string SchoolWebChannel = "school_web";
    private const string DesktopChannel = "desktop";

    public static CurriculumAuthorizationResult EvaluateCreateCurriculum(
        CurriculumPrincipalContext context,
        DateTimeOffset serverNow)
    {
        ArgumentNullException.ThrowIfNull(context);

        var baseline = ValidateSessionBaseline(context);
        if (!baseline.IsAllowed)
            return baseline;

        if (!string.Equals(context.Channel, StaffAdminChannel, StringComparison.Ordinal))
            return CurriculumAuthorizationResult.Denied(
                403,
                ErrorCodes.ChannelNotAllowed,
                "Central curriculum creation is restricted to the staff_admin channel.");

        var hasAuthorRole = context.Roles is not null &&
            context.Roles.Any(grant => grant.IsActive(serverNow, ContentAuthorRole));

        if (!hasAuthorRole)
            return CurriculumAuthorizationResult.Denied(
                403,
                ErrorCodes.StaffRoleRequired,
                "The active content_author role is required to create central curriculum.");

        return CurriculumAuthorizationResult.Allowed();
    }

    public static CurriculumAuthorizationResult EvaluateUpdateCurriculumContent(
        CurriculumPrincipalContext context,
        DateTimeOffset serverNow)
    {
        ArgumentNullException.ThrowIfNull(context);

        var baseline = ValidateSessionBaseline(context);
        if (!baseline.IsAllowed)
            return baseline;

        if (!string.Equals(context.Channel, StaffAdminChannel, StringComparison.Ordinal))
            return CurriculumAuthorizationResult.Denied(
                403,
                ErrorCodes.ChannelNotAllowed,
                "School accounts cannot modify central curriculum content.");

        var hasAuthorRole = context.Roles is not null &&
            context.Roles.Any(grant => grant.IsActive(serverNow, ContentAuthorRole));

        if (!hasAuthorRole)
            return CurriculumAuthorizationResult.Denied(
                403,
                ErrorCodes.StaffRoleRequired,
                "The active content_author role is required to modify central curriculum content.");

        return CurriculumAuthorizationResult.Allowed();
    }

    public static CurriculumAuthorizationResult EvaluatePublishCurriculum(
        CurriculumPrincipalContext context,
        string? authorPrincipalId,
        DateTimeOffset serverNow)
    {
        ArgumentNullException.ThrowIfNull(context);

        var baseline = ValidateSessionBaseline(context);
        if (!baseline.IsAllowed)
            return baseline;

        if (!string.Equals(context.Channel, StaffAdminChannel, StringComparison.Ordinal))
            return CurriculumAuthorizationResult.Denied(
                403,
                ErrorCodes.ChannelNotAllowed,
                "Central curriculum publication is restricted to the staff_admin channel.");

        var hasApproverRole = context.Roles is not null &&
            context.Roles.Any(grant => grant.IsActive(serverNow, ContentApproverRole));

        if (!hasApproverRole)
            return CurriculumAuthorizationResult.Denied(
                403,
                ErrorCodes.StaffRoleRequired,
                "The active content_approver role is required to publish central curriculum.");

        if (!string.IsNullOrWhiteSpace(authorPrincipalId) &&
            string.Equals(context.PrincipalId, authorPrincipalId, StringComparison.Ordinal))
        {
            return CurriculumAuthorizationResult.Denied(
                403,
                ErrorCodes.FourEyesRequired,
                "The curriculum author cannot approve their own curriculum publication.");
        }

        return CurriculumAuthorizationResult.Allowed();
    }

    public static CurriculumAuthorizationResult EvaluateSelectCurriculum(
        CurriculumPrincipalContext context,
        bool isCurriculumPublished,
        DateTimeOffset serverNow)
    {
        ArgumentNullException.ThrowIfNull(context);

        var baseline = ValidateSessionBaseline(context);
        if (!baseline.IsAllowed)
            return baseline;

        if (!string.Equals(context.Channel, SchoolWebChannel, StringComparison.Ordinal))
            return CurriculumAuthorizationResult.Denied(
                403,
                ErrorCodes.ChannelNotAllowed,
                "Selecting curriculum for a school requires the school_web channel.");

        if (context.SchoolId is null || context.SchoolId <= 0)
            return CurriculumAuthorizationResult.Denied(
                403,
                ErrorCodes.ChannelNotAllowed,
                "A valid school context is required to select curriculum.");

        if (!context.IsSchoolAdmin)
            return CurriculumAuthorizationResult.Denied(
                403,
                ErrorCodes.ChannelNotAllowed,
                "Only school administrators can select and assign curriculum for the school.");

        if (!context.HasActiveSchoolLicense)
            return CurriculumAuthorizationResult.Denied(
                403,
                ErrorCodes.LicenseRequired,
                "An active school license is required to select and use curriculum.");

        if (!isCurriculumPublished)
            return CurriculumAuthorizationResult.Denied(
                409,
                ErrorCodes.StateConflict,
                "Only published curriculum revisions can be selected for school use.");

        return CurriculumAuthorizationResult.Allowed();
    }

    public static CurriculumAuthorizationResult EvaluateReadCurriculum(
        CurriculumPrincipalContext context,
        bool isCurriculumPublished,
        DateTimeOffset serverNow)
    {
        ArgumentNullException.ThrowIfNull(context);

        var baseline = ValidateSessionBaseline(context);
        if (!baseline.IsAllowed)
            return baseline;

        if (isCurriculumPublished)
        {
            if (string.Equals(context.Channel, StaffAdminChannel, StringComparison.Ordinal) ||
                string.Equals(context.Channel, SchoolWebChannel, StringComparison.Ordinal) ||
                string.Equals(context.Channel, DesktopChannel, StringComparison.Ordinal))
            {
                return CurriculumAuthorizationResult.Allowed();
            }

            return CurriculumAuthorizationResult.Denied(
                403,
                ErrorCodes.ChannelNotAllowed,
                "Channel is not permitted to read published curriculum.");
        }

        if (!string.Equals(context.Channel, StaffAdminChannel, StringComparison.Ordinal))
            return CurriculumAuthorizationResult.Denied(
                403,
                ErrorCodes.ChannelNotAllowed,
                "Draft curriculum is only accessible to central staff.");

        var hasStaffRole = context.Roles is not null &&
            context.Roles.Any(grant => grant.IsActive(serverNow, ContentAuthorRole) ||
                                       grant.IsActive(serverNow, ContentApproverRole));

        if (!hasStaffRole)
            return CurriculumAuthorizationResult.Denied(
                403,
                ErrorCodes.StaffRoleRequired,
                "An active staff content role is required to read draft curriculum.");

        return CurriculumAuthorizationResult.Allowed();
    }

    private static CurriculumAuthorizationResult ValidateSessionBaseline(CurriculumPrincipalContext context)
    {
        if (!context.IsAuthConfigured)
            return CurriculumAuthorizationResult.Denied(
                503,
                ErrorCodes.AuthNotConfigured,
                "The authentication system is not configured.");

        if (string.IsNullOrWhiteSpace(context.Token) || string.IsNullOrWhiteSpace(context.PrincipalId))
            return CurriculumAuthorizationResult.Denied(
                401,
                ErrorCodes.Unauthorized,
                "A valid authentication token and principal are required.");

        if (context.IsSessionExpired)
            return CurriculumAuthorizationResult.Denied(
                401,
                ErrorCodes.SessionExpired,
                "The session has expired.");

        return CurriculumAuthorizationResult.Allowed();
    }
}
