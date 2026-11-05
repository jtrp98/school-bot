using System.Text.Json;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Services.Implementations.Assessment;

public sealed class CriteriaApprovalService(ISchoolBotRepositoryScope repository)
{
    public async Task<IReadOnlyList<SchoolBotLevelRecord>> ListLevelsAsync(CurriculumPrincipalContext principal, CancellationToken ct)
    {
        var access = CurriculumAuthorizationService.EvaluateReadCurriculum(principal, false, await repository.GetAssessmentTimeAsync(ct));
        if (!access.IsAllowed || principal.Channel != "staff_admin") throw new DomainRuleException(access.ErrorCode ?? ErrorCodes.ChannelNotAllowed, "Central content permission is required.");
        return await repository.ListLevelsAsync(ct);
    }
    public async Task<SchoolBotCurriculumRecord> ApproveAsync(CurriculumPrincipalContext principal, int id, string approvalRef,
        string? etag, Guid key, CancellationToken ct = default)
    {
        if (key == Guid.Empty || string.IsNullOrWhiteSpace(approvalRef) || approvalRef.Length > 1000) throw new DomainRuleException(ErrorCodes.InvalidInput, "Approval reference is required.");
        if (repository is not IIdempotentSchoolBotCommit commit) throw new DomainRuleException(ErrorCodes.ServiceUnavailable, "Atomic criteria storage is required.");
        var result = await commit.CommitWithIdempotencyAsync(new(null, principal.PrincipalId!, "approve_learning_criteria", key,
            CanonicalJson.Hash(new { id, approvalRef, etag })), async (scope, _, now, token) =>
        {
            var current = principal.RecheckAsync is null ? principal : await principal.RecheckAsync(scope, token);
            var access = CurriculumAuthorizationService.EvaluatePublishCurriculum(current, null, now);
            if (current.PrincipalId != principal.PrincipalId || !access.IsAllowed) throw new DomainRuleException(access.ErrorCode ?? ErrorCodes.Unauthorized, "Current approver permission is required.");
        }, async (scope, now, token) =>
        {
            var curriculum = await scope.GetCurriculumByIdAsync(id, token) ?? throw new DomainRuleException(ErrorCodes.NotFound, "Curriculum not found.");
            if (curriculum.SchoolId is not null || curriculum.Lifecycle != "draft") throw new DomainRuleException(ErrorCodes.StateConflict, "A central draft is required.");
            if (string.IsNullOrWhiteSpace(etag)) throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match is required.");
            if (!OpaqueConcurrencyToken.Matches(etag, curriculum.UpdateDate)) throw new DomainRuleException(ErrorCodes.VersionConflict, "Curriculum changed.");
            var contributors = (await scope.ListAuditEventsByEntityAsync("SchoolBotCurriculum", id, token))
                .Where(a => a.Event is "Curriculum.Created" or "Curriculum.MetadataUpdated" or "Curriculum.ContentUpdated" or "Curriculum.Imported").ToArray();
            if (contributors.Length == 0 || contributors.Any(a => a.Actor == principal.PrincipalId))
                throw new DomainRuleException(ErrorCodes.FourEyesRequired, "An independent approver must review these criteria.");
            now = await scope.GetAssessmentTimeAsync(token);
            if (now <= curriculum.UpdateDate) now = curriculum.UpdateDate!.Value.AddTicks(10);
            var saved = await scope.ApproveLearningCriteriaAsync(id, principal.PrincipalId!, now,
                new(0, principal.PrincipalId!, "Criteria.Approved", "SchoolBotCurriculum", id, Guid.NewGuid(), approvalRef,
                    false, null, now, null, now, null), token);
            return new MutationResult<SchoolBotCurriculumRecord>(saved, 200, id, OpaqueConcurrencyToken.ToETag(saved.UpdateDate));
        }, ct);
        return result.Result.Body;
    }
}
