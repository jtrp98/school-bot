using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.API.GrpcServices;

// HTTP status each domain error answers with; it travels in the error trailer and the Gateway uses it.
internal static class ErrorStatus
{
    public static int For(string domainCode) => domainCode switch
    {
        ErrorCodes.VersionConflict => StatusCodes.Status412PreconditionFailed,
        ErrorCodes.PreconditionRequired => StatusCodes.Status428PreconditionRequired,
        ErrorCodes.ServiceUnavailable or ErrorCodes.AuthNotConfigured => StatusCodes.Status503ServiceUnavailable,
        ErrorCodes.Unauthorized or ErrorCodes.SessionExpired => StatusCodes.Status401Unauthorized,
        ErrorCodes.SchoolMismatch or ErrorCodes.ChannelNotAllowed or ErrorCodes.ProfileNotAllowed or ErrorCodes.StaffRoleRequired or
        ErrorCodes.LicenseRequired or ErrorCodes.LicenseInvalid or ErrorCodes.LicenseInactive or
        ErrorCodes.DeviceNotActivated or ErrorCodes.DeviceProofInvalid or ErrorCodes.BindingRevoked =>
            StatusCodes.Status403Forbidden,
        ErrorCodes.NotFound => StatusCodes.Status404NotFound,
        ErrorCodes.DeviceLimit or ErrorCodes.ReleaseExists or ErrorCodes.QuestionKindUnsupported or
        ErrorCodes.IdempotencyExpired or ErrorCodes.IdempotencyConflict or ErrorCodes.StateConflict or
        ErrorCodes.FourEyesRequired or ErrorCodes.CriteriaNotApproved => StatusCodes.Status409Conflict,
        ErrorCodes.PayloadTooLarge => StatusCodes.Status413PayloadTooLarge,
        _ => StatusCodes.Status400BadRequest
    };
}
