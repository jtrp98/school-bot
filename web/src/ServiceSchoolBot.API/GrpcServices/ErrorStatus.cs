using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.API.GrpcServices;

// HTTP status each domain error answers with; it travels in the error trailer and the Gateway uses it.
internal static class ErrorStatus
{
    public static int For(string domainCode) => domainCode switch
    {
        "WRITER_NOT_VERIFIED" or "CALCULATOR_NOT_VERIFIED" or "COEXISTENCE_NOT_VERIFIED" or "GRADE_SOURCE_UNAVAILABLE" or
        "RECEIPT_UNAVAILABLE" or "ROSTER_UNAVAILABLE" or "AUTH_UNAVAILABLE" or "GRADE_WRITE_UNKNOWN" => StatusCodes.Status503ServiceUnavailable,
        "SLOT_UNAVAILABLE" or "SLOT_ALREADY_OWNED" or "MAPPING_LOCKED" or "MAPPING_REQUIRED" or "GRADE_CONFLICT" or
        "PREVIEW_EXPIRED" or "PREVIEW_ALREADY_CONFIRMED" or "PREVIEW_NOT_READY" or "SNAPSHOT_MISMATCH" or "SNAPSHOT_STALE" or
        "PROPOSAL_STALE" or "NEXT_TERM_UNAVAILABLE" or "NEXT_TERM_NOT_AVAILABLE" or "TERM_NOT_ENDED" or
        "SUCCESSOR_EXISTS" or "LEVEL_NOT_CONFIGURED" or "LEVEL_POLICY_NOT_CONFIGURED" or "NEXT_CURRICULUM_NOT_AVAILABLE" or
        "TARGET_NOT_READY" or "ROSTER_EMPTY" or "EXPORT_IN_PROGRESS" => StatusCodes.Status409Conflict,
        "COURSE_RUN_NOT_RESOLVED" or "SOURCE_SCOPE_UNPROVEN" or "ASSESSMENT_METHOD_MISMATCH" => StatusCodes.Status409Conflict,
        ErrorCodes.VersionConflict => StatusCodes.Status412PreconditionFailed,
        ErrorCodes.PreconditionRequired => StatusCodes.Status428PreconditionRequired,
        ErrorCodes.ServiceUnavailable or ErrorCodes.AuthNotConfigured or ErrorCodes.GraderUnavailable => StatusCodes.Status503ServiceUnavailable,
        ErrorCodes.Unauthorized or ErrorCodes.SessionExpired => StatusCodes.Status401Unauthorized,
        ErrorCodes.SchoolMismatch or ErrorCodes.ChannelNotAllowed or ErrorCodes.ProfileNotAllowed or ErrorCodes.StaffRoleRequired or
        ErrorCodes.LicenseRequired or ErrorCodes.LicenseInvalid or ErrorCodes.LicenseInactive or
        ErrorCodes.DeviceNotActivated or ErrorCodes.DeviceProofInvalid or ErrorCodes.BindingRevoked =>
            StatusCodes.Status403Forbidden,
        ErrorCodes.NotFound => StatusCodes.Status404NotFound,
        ErrorCodes.AttemptExpired or ErrorCodes.DeviceLimit or ErrorCodes.ReleaseExists or ErrorCodes.QuestionKindUnsupported or
        ErrorCodes.IdempotencyExpired or ErrorCodes.IdempotencyConflict or ErrorCodes.StateConflict or
        ErrorCodes.FourEyesRequired or ErrorCodes.CriteriaNotApproved => StatusCodes.Status409Conflict,
        ErrorCodes.PayloadTooLarge => StatusCodes.Status413PayloadTooLarge,
        _ => StatusCodes.Status400BadRequest
    };
}
