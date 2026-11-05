namespace ServiceSchoolBot.Domain.Identity;

public sealed record SchoolBotSessionRecord(
    int SchoolBotSessionId,
    string Channel,
    int? AccountId,
    string TokenHash,
    string? ProfileKind,
    int? ProfileId,
    string? LegacyAdminId,
    string? DeviceJkt,
    DateTimeOffset? SourceTokenExpiresAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    bool IsDelete,
    int? CreateBy,
    DateTimeOffset? CreateDate,
    int? UpdateBy,
    DateTimeOffset? UpdateDate,
    int SchoolId);

public sealed record SchoolBotStaffRoleRecord(
    int SchoolBotStaffRoleId,
    int EmployeeId,
    string Role,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidUntil,
    string Reason,
    bool IsDelete,
    int? CreateBy,
    DateTimeOffset? CreateDate,
    int? UpdateBy,
    DateTimeOffset? UpdateDate,
    int SchoolId);

public sealed record LegacyMasterAccountRecord(
    int AccountId,
    int SchoolId,
    string Username,
    string CType,
    bool IsActive);

public sealed record LegacyStudentProfileRecord(
    int StudentId,
    int SchoolId,
    string DisplayName,
    bool IsActive);

public sealed record LegacyEmployeeProfileRecord(
    int EmployeeId,
    int SchoolId,
    string DisplayName,
    bool IsActive,
    bool IsAdmin);
