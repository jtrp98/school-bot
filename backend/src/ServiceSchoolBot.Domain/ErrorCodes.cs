namespace ServiceSchoolBot.Domain;

// One list for the wire contract: the API maps these to HTTP status and clients match on them,
// so a misspelled literal would silently become a generic 400.
public static class ErrorCodes
{
    public const string AttemptExpired = "ATTEMPT_EXPIRED";
    public const string GraderUnavailable = "GRADER_UNAVAILABLE";
    public const string NotFound = "NOT_FOUND";
    public const string DeviceLimit = "DEVICE_LIMIT";
    public const string ReleaseExists = "RELEASE_EXISTS";
    public const string LicenseInvalid = "LICENSE_INVALID";
    public const string LicenseInactive = "LICENSE_INACTIVE";
    public const string DeviceNotActivated = "DEVICE_NOT_ACTIVATED";
    public const string DeviceProofInvalid = "DEVICE_PROOF_INVALID";
    public const string BindingRevoked = "BINDING_REVOKED";
    public const string QuestionKindUnsupported = "QUESTION_KIND_UNSUPPORTED";
    public const string ProfileNotAllowed = "PROFILE_NOT_ALLOWED";
    public const string InvalidInput = "INVALID_INPUT";
    public const string InvalidQuery = "INVALID_QUERY";
    public const string InvalidLicense = "INVALID_LICENSE";
    public const string InvalidRelease = "INVALID_RELEASE";
    public const string InvalidArtifact = "INVALID_ARTIFACT";
    public const string ScoringWeightInvalid = "SCORING_WEIGHT_INVALID";
    public const string PolicyNotConfigured = "POLICY_NOT_CONFIGURED";
    public const string TargetNotReady = "TARGET_NOT_READY";
    public const string AuthNotConfigured = "AUTH_NOT_CONFIGURED";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string SessionExpired = "SESSION_EXPIRED";
    public const string ChannelNotAllowed = "CHANNEL_NOT_ALLOWED";
    public const string StaffRoleRequired = "STAFF_ROLE_REQUIRED";
    public const string IdempotencyExpired = "IDEMPOTENCY_EXPIRED";
    public const string IdempotencyConflict = "IDEMPOTENCY_CONFLICT";
    public const string StateConflict = "STATE_CONFLICT";
    public const string VersionConflict = "VERSION_CONFLICT";
    public const string PreconditionRequired = "PRECONDITION_REQUIRED";
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
    public const string SchoolMismatch = "SCHOOL_MISMATCH";
    public const string LicenseRequired = "LICENSE_REQUIRED";
    public const string FourEyesRequired = "FOUR_EYES_REQUIRED";
    public const string PayloadTooLarge = "PAYLOAD_TOO_LARGE";
    public const string CriteriaNotApproved = "CRITERIA_NOT_APPROVED";
}
