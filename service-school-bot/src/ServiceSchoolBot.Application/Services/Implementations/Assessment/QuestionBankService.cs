using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Services.Implementations.Assessment;

public sealed partial class QuestionBankService(IAssessmentRepository dbAdapter, TimeProvider clock)
{
    public Task<QuestionBankMutationResponse> CreateQuestionBankAsync(
        AssessmentPrincipalContext principal, CreateQuestionBankRequest request,
        Guid? requestId = null, Guid? idempotencyKey = null, CancellationToken ct = default)
    {
        Authorize(principal, false, clock.GetUtcNow());
        if (request is null || request.BankKey == Guid.Empty || request.Revision < 1) QuestionBankValidation.Invalid();
        QuestionBankValidation.ValidateText(request.Title);
        QuestionBankValidation.Validate(request.Title.GetRawText(), request.Questions, request.SourceRef);
        return CommitAsync(principal, "create_question_bank", JsonSerializer.Serialize(request),
            idempotencyKey, 201, async (transaction, now, token) =>
            {
                var record = new SchoolBotQuestionBankRecord(0, request.BankKey, request.Revision,
                    request.Title.GetRawText(), request.Questions, request.SourceRef, "draft", null,
                    false, principal.EmployeeId, now, principal.EmployeeId, now);
                return await transaction.CreateQuestionBankAsync(record,
                    Audit(principal, "Created", "Question bank created", requestId, now), token);
            }, ct);
    }

    public Task<QuestionBankMutationResponse> UpdateQuestionBankDraftAsync(
        AssessmentPrincipalContext principal, int id, UpdateQuestionBankDraftRequest request, string? expectedETag,
        Guid? requestId = null, Guid? idempotencyKey = null, CancellationToken ct = default)
    {
        Authorize(principal, false, clock.GetUtcNow());
        ValidateId(id);
        if (request is null) QuestionBankValidation.Invalid();
        QuestionBankValidation.ValidateText(request.Title);
        QuestionBankValidation.Validate(request.Title.GetRawText(), request.Questions, request.SourceRef);
        RequireETag(expectedETag);
        return CommitAsync(principal, "update_question_bank_draft", id + "|" + JsonSerializer.Serialize(request),
            idempotencyKey, 200, (transaction, now, token) => transaction.UpdateQuestionBankDraftAsync(id,
                request.Title.GetRawText(), request.Questions, request.SourceRef, expectedETag!,
                Audit(principal, "Updated", "Question bank updated", requestId, now), token), ct);
    }

    public Task<QuestionBankMutationResponse> PublishQuestionBankAsync(
        AssessmentPrincipalContext principal, int id, PublishQuestionBankRequest request, string? expectedETag,
        Guid? requestId = null, Guid? idempotencyKey = null, CancellationToken ct = default)
    {
        Authorize(principal, true, clock.GetUtcNow());
        ValidateId(id);
        if (request is null || string.IsNullOrWhiteSpace(request.ApprovalRef) || request.ApprovalRef.Length > 1000)
            QuestionBankValidation.Invalid();
        RequireETag(expectedETag);
        return CommitAsync(principal, "publish_question_bank", id + "|" + request.ApprovalRef,
            idempotencyKey, 200, (transaction, now, token) => transaction.PublishQuestionBankAsync(id,
                expectedETag!, Audit(principal, "Published", request.ApprovalRef, requestId, now), token), ct);
    }

    private async Task<QuestionBankMutationResponse> CommitAsync(
        AssessmentPrincipalContext principal, string operation, string payload, Guid? key, int status,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<SchoolBotQuestionBankRecord>> mutate,
        CancellationToken ct)
    {
        if (dbAdapter is not IIdempotentSchoolBotCommit committer)
            throw new InvalidOperationException("The adapter does not support atomic idempotent commits.");
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        var command = new IdempotencyCommand(null, $"staff_admin:{principal.Session.PrincipalId}",
            operation, key ?? Guid.NewGuid(), hash);
        var committed = await committer.CommitWithIdempotencyAsync(command,
            (_, _, now, _) =>
            {
                Authorize(principal, operation == "publish_question_bank", now);
                return Task.CompletedTask;
            },
            async (transaction, now, token) =>
            {
                var bank = await mutate(transaction, now, token);
                var body = new QuestionBankMutationResponse(bank.SchoolBotQuestionBankId,
                    bank.UpdateDate!.Value, bank.PublishedAt);
                return new MutationResult<QuestionBankMutationResponse>(body, status, body.Id,
                    OpaqueConcurrencyToken.ToETag(body.UpdateDate));
            }, ct);
        return committed.Result.Body;
    }

    private static SchoolBotAuditEventRecord Audit(AssessmentPrincipalContext principal, string action,
        string reason, Guid? requestId, DateTimeOffset now) =>
        new(0, principal.Session.PrincipalId!, "QuestionBank." + action, "SchoolBotQuestionBank", 0,
            requestId ?? Guid.NewGuid(), reason, false, principal.EmployeeId, now, principal.EmployeeId, now);

    private static void Authorize(AssessmentPrincipalContext principal, bool publish, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(principal);
        var access = publish
            ? CurriculumAuthorizationService.EvaluatePublishCurriculum(principal.Session, null, now)
            : CurriculumAuthorizationService.EvaluateCreateCurriculum(principal.Session, now);
        if (!access.IsAllowed) throw new DomainRuleException(access.ErrorCode!, access.Message!);
        if (principal.EmployeeId is null)
            throw new DomainRuleException(ErrorCodes.StaffRoleRequired, "A staff employee identity is required.");
    }

    private static void ValidateId(int id)
    {
        if (id <= 0) QuestionBankValidation.Invalid();
    }

    private static void RequireETag(string? etag)
    {
        if (string.IsNullOrWhiteSpace(etag))
            throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match is required.");
    }
}
