using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceSchoolBot.Application.Common.Model.Projects;

public sealed record ProjectAuthContext(
    bool IsAuthConfigured,
    string? Token,
    bool IsSessionExpired,
    string? Channel,
    int? SchoolId,
    string? OwnerKind,
    int? OwnerId,
    int? UserId = null,
    bool HasActiveLicense = true
);

public sealed record ProjectAuthorizationResult(
    bool IsAllowed,
    int StatusCode,
    string? ErrorCode = null,
    string? Message = null
)
{
    public static ProjectAuthorizationResult Allowed() =>
        new(true, 200);

    public static ProjectAuthorizationResult Denied(int status, string code, string message) =>
        new(false, status, code, message);
}

public sealed record CreateProjectRequest(
    string Mode,
    string Name,
    JsonElement Content
);

public sealed record UpdateProjectRequest(
    string Name,
    JsonElement Content
);

public sealed record CopyProjectRequest(
    string Name,
    [property: JsonPropertyName("source_project_revision_id")] int SourceProjectRevisionId
);

public sealed record ImportGuestProjectRequest(
    [property: JsonPropertyName("local_project_id")] string LocalProjectId,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("local_revision")] int LocalRevision,
    [property: JsonPropertyName("content")] JsonElement Content
);

public sealed record ImportGuestProjectResponse(
    CloudProjectDocumentResponse Document,
    bool Replayed
);

public sealed record CloudProjectDocumentResponse(
    [property: JsonPropertyName("schema")] int Schema,
    [property: JsonPropertyName("project_id")] int ProjectId,
    [property: JsonPropertyName("project_revision_id")] int ProjectRevisionId,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("revision")] int Revision,
    [property: JsonPropertyName("update_date")] DateTimeOffset UpdateDate,
    [property: JsonPropertyName("content")] JsonElement Content
);

public sealed record ProjectMetadataItem(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("project_id")] int ProjectId,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("revision")] int Revision,
    [property: JsonPropertyName("update_date")] DateTimeOffset UpdateDate
);

public sealed record ProjectListResponse(
    [property: JsonPropertyName("items")] IReadOnlyList<ProjectMetadataItem> Items,
    [property: JsonPropertyName("next_cursor")] string? NextCursor
);

public sealed record DeleteProjectResponse(
    [property: JsonPropertyName("project_id")] int ProjectId,
    [property: JsonPropertyName("deleted")] bool Deleted,
    [property: JsonPropertyName("update_date")] DateTimeOffset UpdateDate
);

public sealed class VersionConflictException(DateTimeOffset? currentUpdateDate, string message = "Concurrency token mismatch.")
    : Exception(message)
{
    public DateTimeOffset? CurrentUpdateDate { get; } = currentUpdateDate;
}

public static class ProjectPrincipalStub
{
    public const string DesktopChannel = "desktop";
    public const string SchoolWebChannel = "school_web";
    public const string StaffAdminChannel = "staff_admin";

    public const string StudentToken = "token-desktop-student-valid";
    public const string Student2Token = "token-desktop-student2-valid";
    public const string TeacherToken = "token-desktop-teacher-valid";
    public const string OtherSchoolStudentToken = "token-desktop-other-school-valid";
    public const string ExpiredToken = "token-desktop-expired-valid";
    public const string WrongChannelToken = "token-desktop-wrong-channel-valid";
    public const string NoLicenseToken = "token-desktop-no-license-valid";
    public const string UnconfiguredToken = "token-desktop-unconfigured-valid";

    public static ProjectAuthContext CreateStudent(
        int schoolId = 1001,
        int studentId = 501,
        string token = StudentToken) =>
        new(
            IsAuthConfigured: true,
            Token: token,
            IsSessionExpired: false,
            Channel: DesktopChannel,
            SchoolId: schoolId,
            OwnerKind: "student",
            OwnerId: studentId,
            UserId: studentId,
            HasActiveLicense: true
        );

    public static ProjectAuthContext CreateStudent2(
        int schoolId = 1001,
        int studentId = 502,
        string token = Student2Token) =>
        new(
            IsAuthConfigured: true,
            Token: token,
            IsSessionExpired: false,
            Channel: DesktopChannel,
            SchoolId: schoolId,
            OwnerKind: "student",
            OwnerId: studentId,
            UserId: studentId,
            HasActiveLicense: true
        );

    public static ProjectAuthContext CreateTeacher(
        int schoolId = 1001,
        int employeeId = 902,
        string token = TeacherToken) =>
        new(
            IsAuthConfigured: true,
            Token: token,
            IsSessionExpired: false,
            Channel: DesktopChannel,
            SchoolId: schoolId,
            OwnerKind: "employee",
            OwnerId: employeeId,
            UserId: employeeId,
            HasActiveLicense: true
        );

    public static ProjectAuthContext CreateOtherSchoolStudent(
        int schoolId = 2002,
        int studentId = 601,
        string token = OtherSchoolStudentToken) =>
        new(
            IsAuthConfigured: true,
            Token: token,
            IsSessionExpired: false,
            Channel: DesktopChannel,
            SchoolId: schoolId,
            OwnerKind: "student",
            OwnerId: studentId,
            UserId: studentId,
            HasActiveLicense: true
        );

    public static ProjectAuthContext CreateExpired(string token = ExpiredToken) =>
        new(
            IsAuthConfigured: true,
            Token: token,
            IsSessionExpired: true,
            Channel: DesktopChannel,
            SchoolId: 1001,
            OwnerKind: "student",
            OwnerId: 501,
            UserId: 501,
            HasActiveLicense: true
        );

    public static ProjectAuthContext CreateWrongChannel(string token = WrongChannelToken) =>
        new(
            IsAuthConfigured: true,
            Token: token,
            IsSessionExpired: false,
            Channel: SchoolWebChannel,
            SchoolId: 1001,
            OwnerKind: "student",
            OwnerId: 501,
            UserId: 501,
            HasActiveLicense: true
        );

    public static ProjectAuthContext CreateNoLicense(string token = NoLicenseToken) =>
        new(
            IsAuthConfigured: true,
            Token: token,
            IsSessionExpired: false,
            Channel: DesktopChannel,
            SchoolId: 1001,
            OwnerKind: "student",
            OwnerId: 501,
            UserId: 501,
            HasActiveLicense: false
        );

    public static ProjectAuthContext CreateUnconfigured() =>
        new(
            IsAuthConfigured: false,
            Token: UnconfiguredToken,
            IsSessionExpired: false,
            Channel: DesktopChannel,
            SchoolId: null,
            OwnerKind: null,
            OwnerId: null
        );

    public static ProjectAuthContext CreateAnonymous() =>
        new(
            IsAuthConfigured: true,
            Token: null,
            IsSessionExpired: false,
            Channel: null,
            SchoolId: null,
            OwnerKind: null,
            OwnerId: null
        );
}
