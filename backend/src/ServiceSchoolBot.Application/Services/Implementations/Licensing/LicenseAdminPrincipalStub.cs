using ServiceSchoolBot.Application.Common.Model.Licensing;
namespace ServiceSchoolBot.Application.Services.Implementations.Licensing;

public static class LicenseAdminPrincipalStub
{
    public const string StaffAdminChannel = "staff_admin";
    public const string LicenseAdminRole = "license_admin";
    public const string ReleasePublisherRole = "release_publisher";
    public const string SchoolWebChannel = "school_web";

    public const string ValidAdminToken = "token-staff-license-admin-valid";
    public const string NoRoleToken = "token-staff-norole-valid";
    public const string OtherRoleToken = "token-staff-other-role-valid";
    public const string WrongChannelToken = "token-wrong-channel-valid";
    public const string ExpiredToken = "token-staff-expired-valid";

    public static LicenseAdminAuthContext CreateLicenseAdmin(
        string principal = "staff-admin-01",
        string token = ValidAdminToken,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validUntil = null) =>
        new(
            IsAuthConfigured: true,
            Principal: principal,
            Token: token,
            IsSessionExpired: false,
            Channel: StaffAdminChannel,
            Roles:
            [
                new StaffRoleGrant(
                    LicenseAdminRole,
                    validFrom ?? DateTimeOffset.UtcNow.AddDays(-30),
                    validUntil)
            ]
        );

    public static LicenseAdminAuthContext CreateStaffNoRole(
        string principal = "staff-norole-01",
        string token = NoRoleToken) =>
        new(
            IsAuthConfigured: true,
            Principal: principal,
            Token: token,
            IsSessionExpired: false,
            Channel: StaffAdminChannel,
            Roles: []
        );

    public static LicenseAdminAuthContext CreateStaffOtherRole(
        string principal = "staff-other-01",
        string token = OtherRoleToken) =>
        new(
            IsAuthConfigured: true,
            Principal: principal,
            Token: token,
            IsSessionExpired: false,
            Channel: StaffAdminChannel,
            Roles:
            [
                new StaffRoleGrant(
                    ReleasePublisherRole,
                    DateTimeOffset.UtcNow.AddDays(-30))
            ]
        );

    public static LicenseAdminAuthContext CreateWrongChannel(
        string principal = "school-admin-01",
        string token = WrongChannelToken) =>
        new(
            IsAuthConfigured: true,
            Principal: principal,
            Token: token,
            IsSessionExpired: false,
            Channel: SchoolWebChannel,
            Roles:
            [
                new StaffRoleGrant(
                    LicenseAdminRole,
                    DateTimeOffset.UtcNow.AddDays(-30))
            ]
        );

    public static LicenseAdminAuthContext CreateExpired(
        string principal = "staff-expired-01",
        string token = ExpiredToken) =>
        new(
            IsAuthConfigured: true,
            Principal: principal,
            Token: token,
            IsSessionExpired: true,
            Channel: StaffAdminChannel,
            Roles:
            [
                new StaffRoleGrant(
                    LicenseAdminRole,
                    DateTimeOffset.UtcNow.AddDays(-30))
            ]
        );

    public static LicenseAdminAuthContext CreateUnconfigured(string? token = null) =>
        new(
            IsAuthConfigured: false,
            Principal: "staff-unconfigured",
            Token: token,
            IsSessionExpired: false,
            Channel: StaffAdminChannel,
            Roles: []
        );

    public static LicenseAdminAuthContext CreateAnonymous() =>
        new(
            IsAuthConfigured: true,
            Principal: null,
            Token: null,
            IsSessionExpired: false,
            Channel: null,
            Roles: null
        );
}
