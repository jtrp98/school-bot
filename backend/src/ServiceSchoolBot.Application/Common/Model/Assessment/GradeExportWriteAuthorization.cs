namespace ServiceSchoolBot.Application.Common.Model.Assessment;

// Supplied by the reviewed deployment composition, never bound from HTTP or a configuration boolean.
public sealed record GradeExportWriteAuthorization(string ApprovedBy, DateTimeOffset ApprovedAt, DateTimeOffset ExpiresAt,
    string CalculatorVersion, string WriterEvidence, string CalculatorEvidence, string CoexistenceEvidence,
    string TargetEvidence, string AuthorizationEvidence, string DatabaseEvidence);
