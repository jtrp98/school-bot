using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Tests;

public sealed class CurriculumAuthorizationTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CentralStaffAuthorCanCreateCurriculum()
    {
        var principal = CurriculumPrincipalStub.CreateStaffAuthor();
        var result = CurriculumAuthorizationService.EvaluateCreateCurriculum(principal, FixedNow);

        Assert.True(result.IsAllowed);
        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public void CentralStaffAuthorCanUpdateCurriculumContent()
    {
        var principal = CurriculumPrincipalStub.CreateStaffAuthor();
        var result = CurriculumAuthorizationService.EvaluateUpdateCurriculumContent(principal, FixedNow);

        Assert.True(result.IsAllowed);
        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public void CentralStaffWithoutAuthorRoleCannotCreateOrUpdateCurriculum()
    {
        var principal = CurriculumPrincipalStub.CreateStaffNoRole();

        var createResult = CurriculumAuthorizationService.EvaluateCreateCurriculum(principal, FixedNow);
        Assert.False(createResult.IsAllowed);
        Assert.Equal(403, createResult.StatusCode);
        Assert.Equal(ErrorCodes.StaffRoleRequired, createResult.ErrorCode);

        var updateResult = CurriculumAuthorizationService.EvaluateUpdateCurriculumContent(principal, FixedNow);
        Assert.False(updateResult.IsAllowed);
        Assert.Equal(403, updateResult.StatusCode);
        Assert.Equal(ErrorCodes.StaffRoleRequired, updateResult.ErrorCode);
    }

    [Fact]
    public void CentralStaffApproverCanPublishWhenAuthorIsDifferent()
    {
        var approver = CurriculumPrincipalStub.CreateStaffApprover("staff-approver-99");
        var result = CurriculumAuthorizationService.EvaluatePublishCurriculum(approver, "staff-author-01", FixedNow);

        Assert.True(result.IsAllowed);
        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public void CentralStaffAuthorCannotPublishOwnCurriculumUnderFourEyesRule()
    {
        var author = CurriculumPrincipalStub.CreateStaffAuthor("staff-same-01");
        var approverWithAuthorPrincipal = new CurriculumPrincipalContext(
            IsAuthConfigured: true,
            PrincipalId: "staff-same-01",
            Token: "token-valid",
            IsSessionExpired: false,
            Channel: CurriculumPrincipalStub.StaffAdminChannel,
            Roles: new[]
            {
                new CurriculumStaffRoleGrant(
                    CurriculumPrincipalStub.StaffApproverRole,
                    FixedNow.AddDays(-1))
            }
        );

        var result = CurriculumAuthorizationService.EvaluatePublishCurriculum(
            approverWithAuthorPrincipal, "staff-same-01", FixedNow);

        Assert.False(result.IsAllowed);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal(ErrorCodes.FourEyesRequired, result.ErrorCode);
    }

    [Fact]
    public void CentralStaffWithoutApproverRoleCannotPublish()
    {
        var authorOnly = CurriculumPrincipalStub.CreateStaffAuthor("staff-author-01");
        var result = CurriculumAuthorizationService.EvaluatePublishCurriculum(
            authorOnly, "another-author", FixedNow);

        Assert.False(result.IsAllowed);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal(ErrorCodes.StaffRoleRequired, result.ErrorCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SchoolAccountOnSchoolWebCannotCreateOrUpdateOrPublishCentralCurriculum(bool isSchoolAdmin)
    {
        var principal = isSchoolAdmin
            ? CurriculumPrincipalStub.CreateSchoolAdmin(1001, "admin-01")
            : CurriculumPrincipalStub.CreateSchoolTeacher(1001, "teacher-01");

        var createResult = CurriculumAuthorizationService.EvaluateCreateCurriculum(principal, FixedNow);
        Assert.False(createResult.IsAllowed);
        Assert.Equal(403, createResult.StatusCode);
        Assert.Equal(ErrorCodes.ChannelNotAllowed, createResult.ErrorCode);

        var updateResult = CurriculumAuthorizationService.EvaluateUpdateCurriculumContent(principal, FixedNow);
        Assert.False(updateResult.IsAllowed);
        Assert.Equal(403, updateResult.StatusCode);
        Assert.Equal(ErrorCodes.ChannelNotAllowed, updateResult.ErrorCode);

        var publishResult = CurriculumAuthorizationService.EvaluatePublishCurriculum(
            principal, "staff-author-01", FixedNow);
        Assert.False(publishResult.IsAllowed);
        Assert.Equal(403, publishResult.StatusCode);
        Assert.Equal(ErrorCodes.ChannelNotAllowed, publishResult.ErrorCode);
    }

    [Fact]
    public void SchoolAdminWithActiveLicenseCanSelectPublishedCurriculum()
    {
        var admin = CurriculumPrincipalStub.CreateSchoolAdmin(1001, "admin-01", hasActiveLicense: true);
        var result = CurriculumAuthorizationService.EvaluateSelectCurriculum(admin, isCurriculumPublished: true, FixedNow);

        Assert.True(result.IsAllowed);
        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public void SchoolAdminCannotSelectUnpublishedDraftCurriculum()
    {
        var admin = CurriculumPrincipalStub.CreateSchoolAdmin(1001, "admin-01", hasActiveLicense: true);
        var result = CurriculumAuthorizationService.EvaluateSelectCurriculum(admin, isCurriculumPublished: false, FixedNow);

        Assert.False(result.IsAllowed);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal(ErrorCodes.StateConflict, result.ErrorCode);
    }

    [Fact]
    public void SchoolAdminWithoutActiveLicenseCannotSelectCurriculum()
    {
        var adminWithoutLicense = CurriculumPrincipalStub.CreateSchoolAdmin(1001, "admin-01", hasActiveLicense: false);
        var result = CurriculumAuthorizationService.EvaluateSelectCurriculum(adminWithoutLicense, isCurriculumPublished: true, FixedNow);

        Assert.False(result.IsAllowed);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal(ErrorCodes.LicenseRequired, result.ErrorCode);
    }

    [Fact]
    public void SchoolTeacherCannotSelectCurriculum()
    {
        var teacher = CurriculumPrincipalStub.CreateSchoolTeacher(1001, "teacher-01", hasActiveLicense: true);
        var result = CurriculumAuthorizationService.EvaluateSelectCurriculum(teacher, isCurriculumPublished: true, FixedNow);

        Assert.False(result.IsAllowed);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal(ErrorCodes.ChannelNotAllowed, result.ErrorCode);
    }

    [Fact]
    public void DesktopStudentCannotCreateUpdatePublishOrSelectCurriculum()
    {
        var student = CurriculumPrincipalStub.CreateDesktopStudent();

        var createResult = CurriculumAuthorizationService.EvaluateCreateCurriculum(student, FixedNow);
        Assert.False(createResult.IsAllowed);
        Assert.Equal(403, createResult.StatusCode);

        var updateResult = CurriculumAuthorizationService.EvaluateUpdateCurriculumContent(student, FixedNow);
        Assert.False(updateResult.IsAllowed);
        Assert.Equal(403, updateResult.StatusCode);

        var publishResult = CurriculumAuthorizationService.EvaluatePublishCurriculum(student, "author", FixedNow);
        Assert.False(publishResult.IsAllowed);
        Assert.Equal(403, publishResult.StatusCode);

        var selectResult = CurriculumAuthorizationService.EvaluateSelectCurriculum(student, true, FixedNow);
        Assert.False(selectResult.IsAllowed);
        Assert.Equal(403, selectResult.StatusCode);
    }

    [Fact]
    public void ExpiredSessionRejectsAllCurriculumOperations()
    {
        var expiredStaff = CurriculumPrincipalStub.CreateExpired(CurriculumPrincipalStub.CreateStaffAuthor());
        var expiredSchool = CurriculumPrincipalStub.CreateExpired(CurriculumPrincipalStub.CreateSchoolAdmin());

        var createResult = CurriculumAuthorizationService.EvaluateCreateCurriculum(expiredStaff, FixedNow);
        Assert.False(createResult.IsAllowed);
        Assert.Equal(401, createResult.StatusCode);
        Assert.Equal(ErrorCodes.SessionExpired, createResult.ErrorCode);

        var selectResult = CurriculumAuthorizationService.EvaluateSelectCurriculum(expiredSchool, true, FixedNow);
        Assert.False(selectResult.IsAllowed);
        Assert.Equal(401, selectResult.StatusCode);
        Assert.Equal(ErrorCodes.SessionExpired, selectResult.ErrorCode);
    }

    [Theory]
    [InlineData(null, "token")]
    [InlineData("", "token")]
    [InlineData("principal", null)]
    [InlineData("principal", "")]
    public void MissingPrincipalOrTokenRejectsWithUnauthorized(string? principalId, string? token)
    {
        var invalid = new CurriculumPrincipalContext(
            IsAuthConfigured: true,
            PrincipalId: principalId,
            Token: token,
            IsSessionExpired: false,
            Channel: CurriculumPrincipalStub.StaffAdminChannel,
            Roles: new[] { new CurriculumStaffRoleGrant(CurriculumPrincipalStub.StaffAuthorRole, FixedNow.AddDays(-1)) }
        );

        var result = CurriculumAuthorizationService.EvaluateCreateCurriculum(invalid, FixedNow);
        Assert.False(result.IsAllowed);
        Assert.Equal(401, result.StatusCode);
        Assert.Equal(ErrorCodes.Unauthorized, result.ErrorCode);
    }

    [Fact]
    public void UnconfiguredAuthRejectsWithServiceUnavailable()
    {
        var unconfigured = CurriculumPrincipalStub.CreateUnconfigured(CurriculumPrincipalStub.CreateStaffAuthor());

        var result = CurriculumAuthorizationService.EvaluateCreateCurriculum(unconfigured, FixedNow);
        Assert.False(result.IsAllowed);
        Assert.Equal(503, result.StatusCode);
        Assert.Equal(ErrorCodes.AuthNotConfigured, result.ErrorCode);
    }

    [Fact]
    public void StaffRoleExpiredByDateOrDeletedIsRejected()
    {
        var expiredRole = new CurriculumStaffRoleGrant(
            CurriculumPrincipalStub.StaffAuthorRole,
            ValidFrom: FixedNow.AddDays(-10),
            ValidUntil: FixedNow.AddDays(-1)
        );

        var contextWithExpired = new CurriculumPrincipalContext(
            IsAuthConfigured: true,
            PrincipalId: "staff-01",
            Token: "token-valid",
            IsSessionExpired: false,
            Channel: CurriculumPrincipalStub.StaffAdminChannel,
            Roles: new[] { expiredRole }
        );

        var expiredResult = CurriculumAuthorizationService.EvaluateCreateCurriculum(contextWithExpired, FixedNow);
        Assert.False(expiredResult.IsAllowed);
        Assert.Equal(403, expiredResult.StatusCode);
        Assert.Equal(ErrorCodes.StaffRoleRequired, expiredResult.ErrorCode);

        var deletedRole = new CurriculumStaffRoleGrant(
            CurriculumPrincipalStub.StaffAuthorRole,
            ValidFrom: FixedNow.AddDays(-10),
            ValidUntil: FixedNow.AddDays(10),
            IsDelete: true
        );

        var contextWithDeleted = contextWithExpired with { Roles = new[] { deletedRole } };
        var deletedResult = CurriculumAuthorizationService.EvaluateCreateCurriculum(contextWithDeleted, FixedNow);
        Assert.False(deletedResult.IsAllowed);
        Assert.Equal(403, deletedResult.StatusCode);
        Assert.Equal(ErrorCodes.StaffRoleRequired, deletedResult.ErrorCode);
    }

    [Fact]
    public void ReadPublishedCurriculumAllowsCentralStaffSchoolAndDesktop()
    {
        var staff = CurriculumPrincipalStub.CreateStaffAuthor();
        var school = CurriculumPrincipalStub.CreateSchoolAdmin();
        var desktop = CurriculumPrincipalStub.CreateDesktopStudent();

        Assert.True(CurriculumAuthorizationService.EvaluateReadCurriculum(staff, true, FixedNow).IsAllowed);
        Assert.True(CurriculumAuthorizationService.EvaluateReadCurriculum(school, true, FixedNow).IsAllowed);
        Assert.True(CurriculumAuthorizationService.EvaluateReadCurriculum(desktop, true, FixedNow).IsAllowed);
    }

    [Fact]
    public void ReadDraftCurriculumDeniedForSchoolAndDesktop()
    {
        var school = CurriculumPrincipalStub.CreateSchoolAdmin();
        var desktop = CurriculumPrincipalStub.CreateDesktopStudent();

        var schoolRead = CurriculumAuthorizationService.EvaluateReadCurriculum(school, false, FixedNow);
        Assert.False(schoolRead.IsAllowed);
        Assert.Equal(403, schoolRead.StatusCode);
        Assert.Equal(ErrorCodes.ChannelNotAllowed, schoolRead.ErrorCode);

        var desktopRead = CurriculumAuthorizationService.EvaluateReadCurriculum(desktop, false, FixedNow);
        Assert.False(desktopRead.IsAllowed);
        Assert.Equal(403, desktopRead.StatusCode);
        Assert.Equal(ErrorCodes.ChannelNotAllowed, desktopRead.ErrorCode);
    }
}
