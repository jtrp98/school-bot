using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
namespace ServiceSchoolBot.Application.Common.Model.Curriculum;

public sealed record CurriculumStaffRoleGrant(
    string Role,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidUntil = null,
    bool IsDelete = false
)
{
    public bool IsActive(DateTimeOffset at, string requiredRole) =>
        !IsDelete &&
        string.Equals(Role, requiredRole, StringComparison.Ordinal) &&
        at >= ValidFrom &&
        (ValidUntil is null || at < ValidUntil.Value);
}

public sealed record CurriculumPrincipalContext(
    bool IsAuthConfigured,
    string? PrincipalId,
    string? Token,
    bool IsSessionExpired,
    string? Channel,
    long? SchoolId = null,
    bool IsSchoolAdmin = false,
    bool HasActiveSchoolLicense = false,
    IReadOnlyList<CurriculumStaffRoleGrant>? Roles = null,
    Func<ServiceSchoolBot.Application.Databases.Repository.Interfaces.ISchoolBotRepositoryScope, CancellationToken,
        Task<CurriculumPrincipalContext>>? RecheckAsync = null
);

public sealed record CurriculumAuthorizationResult(
    bool IsAllowed,
    int StatusCode,
    string? ErrorCode = null,
    string? Message = null
)
{
    public static CurriculumAuthorizationResult Allowed() =>
        new(true, 200);

    public static CurriculumAuthorizationResult Denied(int status, string code, string message) =>
        new(false, status, code, message);
}

public static class CurriculumPrincipalStub
{
    public const string StaffAuthorRole = "content_author";
    public const string StaffApproverRole = "content_approver";
    public const string StaffAdminChannel = "staff_admin";
    public const string SchoolWebChannel = "school_web";
    public const string DesktopChannel = "desktop";

    public static CurriculumPrincipalContext CreateStaffAuthor(
        string principalId = "staff-author-01",
        string token = "token-staff-author-valid",
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validUntil = null) =>
        new(
            IsAuthConfigured: true,
            PrincipalId: principalId,
            Token: token,
            IsSessionExpired: false,
            Channel: StaffAdminChannel,
            SchoolId: null,
            IsSchoolAdmin: false,
            HasActiveSchoolLicense: false,
            Roles: new[]
            {
                new CurriculumStaffRoleGrant(
                    StaffAuthorRole,
                    validFrom ?? DateTimeOffset.UtcNow.AddDays(-30),
                    validUntil)
            }
        );

    public static CurriculumPrincipalContext CreateStaffApprover(
        string principalId = "staff-approver-01",
        string token = "token-staff-approver-valid",
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validUntil = null) =>
        new(
            IsAuthConfigured: true,
            PrincipalId: principalId,
            Token: token,
            IsSessionExpired: false,
            Channel: StaffAdminChannel,
            SchoolId: null,
            IsSchoolAdmin: false,
            HasActiveSchoolLicense: false,
            Roles: new[]
            {
                new CurriculumStaffRoleGrant(
                    StaffApproverRole,
                    validFrom ?? DateTimeOffset.UtcNow.AddDays(-30),
                    validUntil)
            }
        );

    public static CurriculumPrincipalContext CreateStaffNoRole(
        string principalId = "staff-norole-01",
        string token = "token-staff-norole-valid") =>
        new(
            IsAuthConfigured: true,
            PrincipalId: principalId,
            Token: token,
            IsSessionExpired: false,
            Channel: StaffAdminChannel,
            SchoolId: null,
            IsSchoolAdmin: false,
            HasActiveSchoolLicense: false,
            Roles: Array.Empty<CurriculumStaffRoleGrant>()
        );

    public static CurriculumPrincipalContext CreateSchoolAdmin(
        long schoolId = 1001,
        string principalId = "school-admin-01",
        string token = "token-school-admin-valid",
        bool hasActiveLicense = true) =>
        new(
            IsAuthConfigured: true,
            PrincipalId: principalId,
            Token: token,
            IsSessionExpired: false,
            Channel: SchoolWebChannel,
            SchoolId: schoolId,
            IsSchoolAdmin: true,
            HasActiveSchoolLicense: hasActiveLicense,
            Roles: null
        );

    public static CurriculumPrincipalContext CreateSchoolTeacher(
        long schoolId = 1001,
        string principalId = "school-teacher-01",
        string token = "token-school-teacher-valid",
        bool hasActiveLicense = true) =>
        new(
            IsAuthConfigured: true,
            PrincipalId: principalId,
            Token: token,
            IsSessionExpired: false,
            Channel: SchoolWebChannel,
            SchoolId: schoolId,
            IsSchoolAdmin: false,
            HasActiveSchoolLicense: hasActiveLicense,
            Roles: null
        );

    public static CurriculumPrincipalContext CreateDesktopStudent(
        string principalId = "student-01",
        string token = "token-student-valid") =>
        new(
            IsAuthConfigured: true,
            PrincipalId: principalId,
            Token: token,
            IsSessionExpired: false,
            Channel: DesktopChannel,
            SchoolId: null,
            IsSchoolAdmin: false,
            HasActiveSchoolLicense: true,
            Roles: null
        );

    public static CurriculumPrincipalContext CreateExpired(CurriculumPrincipalContext baseContext) =>
        baseContext with { IsSessionExpired = true };

    public static CurriculumPrincipalContext CreateUnconfigured(CurriculumPrincipalContext baseContext) =>
        baseContext with { IsAuthConfigured = false };
}
