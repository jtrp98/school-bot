using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Services.Implementations.Curriculum;

public sealed partial class CurriculumService
{
    public async Task<CurriculumMutationResponse> PublishCurriculumAsync(
        CurriculumPrincipalContext principal,
        int id,
        PublishCurriculumRequest request,
        string? expectedETag,
        Guid? requestId = null,
        Guid? idempotencyKey = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(request);

        var now = clock.GetUtcNow();
        var authCheck = CurriculumAuthorizationService.EvaluatePublishCurriculum(principal, null, now);
        if (!authCheck.IsAllowed)
            throw new DomainRuleException(authCheck.ErrorCode!, authCheck.Message!);

        if (string.IsNullOrWhiteSpace(request.ApprovalRef))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "approval_ref is required.");

        if (request.ApprovalRef.Length > MaxRefLength)
            throw new DomainRuleException(ErrorCodes.InvalidInput, $"approval_ref must not exceed {MaxRefLength} characters.");

        if (string.IsNullOrWhiteSpace(expectedETag))
            throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match header is required.");

        var operation = "publish_curriculum";
        var requestHash = ComputeSha256($"{id}|{request.ApprovalRef}");

        return await CommitMutationAsync(principal, operation, requestHash, idempotencyKey, 200,
            async (transaction, commitNow, token) =>
            {
                var existing = await transaction.GetCurriculumByIdAsync(id, token);
                if (existing is null)
                    throw new DomainRuleException(ErrorCodes.NotFound, "Curriculum not found.");

                if (string.Equals(existing.Lifecycle, "published", StringComparison.OrdinalIgnoreCase))
                {
                    var audits = await transaction.ListAuditEventsByEntityAsync("SchoolBotCurriculum", id, token);
                    var pubAudit = audits.FirstOrDefault(a =>
                        string.Equals(a.Event, "Curriculum.Published", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(a.Reason, request.ApprovalRef, StringComparison.Ordinal));

                    if (pubAudit is not null)
                    {
                        return new CurriculumMutationResponse(existing.SchoolBotCurriculumId, existing.UpdateDate!.Value, existing.PublishedAt);
                    }

                    throw new DomainRuleException(ErrorCodes.StateConflict, "Curriculum is already published.");
                }

                if (!string.Equals(existing.Lifecycle, "draft", StringComparison.OrdinalIgnoreCase))
                    throw new DomainRuleException(ErrorCodes.StateConflict, "Curriculum must be in draft state to publish.");

                if (!OpaqueConcurrencyToken.Matches(expectedETag, existing.UpdateDate))
                    throw new DomainRuleException(ErrorCodes.VersionConflict, "Concurrency token mismatch.");

                var lessons = await transaction.ListLessonsByCurriculumIdAsync(id, token);
                foreach (var lesson in lessons)
                {
                    var missions = await transaction.ListMissionsByLessonIdAsync(lesson.SchoolBotLessonId, token);
                    foreach (var mission in missions)
                    {
                        var criteriaList = await transaction.ListCriteriaByMissionIdAsync(mission.SchoolBotMissionId, token);
                        foreach (var crit in criteriaList)
                        {
                            if (string.IsNullOrWhiteSpace(crit.ApprovedBy) || !crit.ApprovedAt.HasValue)
                            {
                                throw new DomainRuleException(ErrorCodes.CriteriaNotApproved, "All mission criteria must be approved before publication.");
                            }
                        }
                    }
                }

                var allAudits = await transaction.ListAuditEventsByEntityAsync("SchoolBotCurriculum", id, token);
                var contributorActors = allAudits
                    .Where(a => a.Event is "Curriculum.Created" or "Curriculum.MetadataUpdated" or "Curriculum.ContentUpdated")
                    .Select(a => a.Actor)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (contributorActors.Count == 0)
                    throw new DomainRuleException(ErrorCodes.FourEyesRequired, "Audit history is incomplete. Cannot verify four-eyes publication constraint.");

                if (contributorActors.Contains(principal.PrincipalId!))
                    throw new DomainRuleException(ErrorCodes.FourEyesRequired, "The curriculum author or editor cannot approve their own curriculum publication.");

                var publishAudit = new SchoolBotAuditEventRecord(
                    SchoolBotAuditEventId: 0,
                    Actor: principal.PrincipalId!,
                    Event: "Curriculum.Published",
                    EntityType: "SchoolBotCurriculum",
                    EntityId: id,
                    RequestId: requestId ?? Guid.NewGuid(),
                    Reason: request.ApprovalRef,
                    IsDelete: false,
                    CreateBy: null,
                    CreateDate: commitNow,
                    UpdateBy: null,
                    UpdateDate: commitNow,
                    SchoolId: null
                );

                var published = await transaction.PublishCurriculumAsync(id, request.ApprovalRef, expectedETag, publishAudit, token);
                var response = new CurriculumMutationResponse(published.SchoolBotCurriculumId, published.UpdateDate!.Value, published.PublishedAt);
                return response;
            }, ct);
    }

}
