using System.Globalization;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;

namespace ServiceSchoolBot.API.GrpcServices;

public sealed class GatewayExportPrincipalResolver(IAssessmentAuthResolver principals,
    IAssessmentAuthority authority, ISchoolBotRepositoryScope repository) : IExportPrincipalResolver
{
    public async Task<AssessmentPrincipalContext> ResolveAsync(ScoreExportRecord export, CancellationToken ct)
    {
        var expected = string.Create(CultureInfo.InvariantCulture, $"{export.SchoolId}:{export.EmployeeId}");
        if (export.SchoolId <= 0 || export.EmployeeId <= 0 || export.ConfirmedBy != expected)
            throw new DomainRuleException(ErrorCodes.Unauthorized, "The stored confirming teacher identity is inconsistent.");
        // EmployeeId is a profile key, not a fabricated master-account ID or a cached token.
        var principal = await principals.ResolveAsync(new GatewayIdentity(export.SchoolId, 0,
            GatewayIdentity.Employee, null, false, export.EmployeeId, null), ct);
        if (principal.Session.PrincipalId != expected || principal.Session.IsSessionExpired ||
            principal.ProfileKind != GatewayIdentity.Employee || principal.EmployeeId != export.EmployeeId ||
            !principal.Session.HasActiveSchoolLicense)
            throw new DomainRuleException(ErrorCodes.Unauthorized, "The original confirming teacher is no longer active.");
        var preview = await repository.GetScorePreviewAsync(export.PreviewId, ct)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Export preview not found.");
        var run = await repository.GetCourseRunByIdAsync(preview.CourseRunId, ct)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Course run not found.");
        if (preview.SchoolId != export.SchoolId || run.SchoolId != export.SchoolId ||
            !await authority.CanReadRunAsync(principal, run, ct))
            throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "The original teacher's current course assignment is required.");
        return principal;
    }
}
