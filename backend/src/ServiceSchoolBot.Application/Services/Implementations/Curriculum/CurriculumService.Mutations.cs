using ServiceSchoolBot.Application.Common.Model.Curriculum;
using System.Text.Json;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Services.Implementations.Curriculum;

public sealed partial class CurriculumService
{
    public async Task<CurriculumMutationResponse> CreateCurriculumAsync(
        CurriculumPrincipalContext principal,
        CreateCurriculumRequest request,
        Guid? requestId = null,
        Guid? idempotencyKey = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(request);

        var now = clock.GetUtcNow();
        var authCheck = CurriculumAuthorizationService.EvaluateCreateCurriculum(principal, now);
        if (!authCheck.IsAllowed)
            throw new DomainRuleException(authCheck.ErrorCode!, authCheck.Message!);

        ValidateCreateRequest(request);

        var operation = "create_curriculum";
        var requestHash = ComputeSha256(JsonSerializer.Serialize(request));

        return await CommitMutationAsync(principal, operation, requestHash, idempotencyKey, 201,
            async (transaction, commitNow, token) =>
            {
                var existing = await transaction.GetCurriculumByKeyAndRevisionAsync(request.CurriculumKey, request.Revision, token);
                if (existing is not null)
                    throw new DomainRuleException(ErrorCodes.StateConflict, "A curriculum revision with this key and revision already exists.");

                var record = new SchoolBotCurriculumRecord(
                    SchoolBotCurriculumId: 0,
                    CurriculumKey: request.CurriculumKey,
                    Revision: request.Revision,
                    LevelId: request.LevelId,
                    Title: request.Title,
                    RecommendedGrades: request.RecommendedGrades,
                    Subjects: request.Subjects,
                    SourceRef: request.SourceRef,
                    Lifecycle: "draft",
                    PublishedAt: null,
                    IsDelete: false,
                    CreateBy: null,
                    CreateDate: commitNow,
                    UpdateBy: null,
                    UpdateDate: commitNow,
                    SchoolId: null
                );

                var audit = new SchoolBotAuditEventRecord(
                    SchoolBotAuditEventId: 0,
                    Actor: principal.PrincipalId!,
                    Event: "Curriculum.Created",
                    EntityType: "SchoolBotCurriculum",
                    EntityId: 0,
                    RequestId: requestId ?? Guid.NewGuid(),
                    Reason: "Curriculum created",
                    IsDelete: false,
                    CreateBy: null,
                    CreateDate: commitNow,
                    UpdateBy: null,
                    UpdateDate: commitNow,
                    SchoolId: null
                );

                var created = await transaction.CreateCurriculumAsync(record, audit, token);
                var response = new CurriculumMutationResponse(created.SchoolBotCurriculumId, created.UpdateDate!.Value);
                return response;
            }, ct);
    }

    public async Task<CurriculumMutationResponse> UpdateMetadataAsync(
        CurriculumPrincipalContext principal,
        int id,
        CurriculumMetadataInput request,
        string? expectedETag,
        Guid? requestId = null,
        Guid? idempotencyKey = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(request);

        var now = clock.GetUtcNow();
        var authCheck = CurriculumAuthorizationService.EvaluateUpdateCurriculumContent(principal, now);
        if (!authCheck.IsAllowed)
            throw new DomainRuleException(authCheck.ErrorCode!, authCheck.Message!);

        if (string.IsNullOrWhiteSpace(expectedETag))
            throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match header is required.");

        ValidateMetadataInput(request);

        var operation = "update_curriculum_metadata";
        var requestHash = ComputeSha256($"{id}|{JsonSerializer.Serialize(request)}");

        return await CommitMutationAsync(principal, operation, requestHash, idempotencyKey, 200,
            async (transaction, commitNow, token) =>
            {
                var audit = new SchoolBotAuditEventRecord(
                    SchoolBotAuditEventId: 0,
                    Actor: principal.PrincipalId!,
                    Event: "Curriculum.MetadataUpdated",
                    EntityType: "SchoolBotCurriculum",
                    EntityId: id,
                    RequestId: requestId ?? Guid.NewGuid(),
                    Reason: "Curriculum metadata updated",
                    IsDelete: false,
                    CreateBy: null,
                    CreateDate: commitNow,
                    UpdateBy: null,
                    UpdateDate: commitNow,
                    SchoolId: null
                );

                var updated = await transaction.UpdateCurriculumMetadataAsync(
                    id,
                    request.LevelId,
                    request.Title,
                    request.SourceRef,
                    request.RecommendedGrades,
                    request.Subjects,
                    expectedETag,
                    audit,
                    token);

                var response = new CurriculumMutationResponse(updated.SchoolBotCurriculumId, updated.UpdateDate!.Value);
                return response;
            }, ct);
    }

}
