using Grpc.Core;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using static ServiceSchoolBot.API.GrpcServices.AssessmentProtoMapper;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.API.GrpcServices;

// Follows AssessmentEndpoints: authenticate, then reject the body, then the idempotency key, then call
// the service; a version conflict reports the bank's current ETag.
public sealed class AssessmentGrpcService(
    SchoolBotGrpcRunner runner,
    QuestionBankService banks,
    AssessmentReadService reads,
    IAssessmentAuthResolver authResolver,
    IAssessmentRepository repository) : Rpc.AssessmentService.AssessmentServiceBase
{
    public override Task<Rpc.ProgressResponse> GetProgress(Rpc.GetProgressRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
            ToProto(await reads.GetProgressAsync(principal, request.HasCourseRunId ? request.CourseRunId : null,
                request.HasCurriculumId ? request.CurriculumId : null, request.HasMode ? request.Mode : null,
                call.CancellationToken)));

    public override Task<Rpc.QuestionBankMutationResponse> CreateQuestionBank(
        Rpc.CreateQuestionBankRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            var body = new CreateQuestionBankRequest(Guid.Parse(request.BankKey), request.Revision,
                Json(request.HasTitleJson, request.TitleJson), ToDomain(request.Questions)!,
                request.HasSourceRef ? request.SourceRef : null!);
            return ToMutation(await banks.CreateQuestionBankAsync(principal, body, call.RequestId,
                RequestKey(request.IdempotencyKey), call.CancellationToken));
        });

    public override Task<Rpc.QuestionBankMutationResponse> UpdateQuestionBank(
        Rpc.UpdateQuestionBankRequest request, ServerCallContext context) =>
        Run(context, request.Id, async (call, principal) =>
        {
            RejectBody(request.Body);
            var body = new UpdateQuestionBankDraftRequest(Json(request.HasTitleJson, request.TitleJson),
                ToDomain(request.Questions)!, request.HasSourceRef ? request.SourceRef : null!);
            return ToMutation(await banks.UpdateQuestionBankDraftAsync(principal, request.Id, body, request.IfMatch,
                call.RequestId, RequestKey(request.IdempotencyKey), call.CancellationToken));
        });

    public override Task<Rpc.QuestionBankMutationResponse> PublishQuestionBank(
        Rpc.PublishQuestionBankRequest request, ServerCallContext context) =>
        Run(context, request.Id, async (call, principal) =>
        {
            RejectBody(request.Body);
            var body = new PublishQuestionBankRequest(request.HasApprovalRef ? request.ApprovalRef : null);
            return ToMutation(await banks.PublishQuestionBankAsync(principal, request.Id, body, request.IfMatch,
                call.RequestId, RequestKey(request.IdempotencyKey), call.CancellationToken));
        });

    public override Task<Rpc.QuestionBankResponse> GetQuestionBank(
        Rpc.GetQuestionBankRequest request, ServerCallContext context) =>
        Run(context, request.Id, async (call, principal) =>
        {
            var view = await banks.GetQuestionBankAsync(principal, request.Id, call.CancellationToken);
            return ToProto(view, OpaqueConcurrencyToken.ToETag(view.UpdateDate));
        });

    public override Task<Rpc.QuestionBankListResponse> ListQuestionBanks(
        Rpc.ListQuestionBanksRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
            ToProto(await banks.ListQuestionBanksAsync(principal, request.HasBankKey ? Guid.Parse(request.BankKey) : null,
                request.HasLifecycle ? request.Lifecycle : null, request.HasLimit ? request.Limit : 50,
                request.HasCursor ? request.Cursor : null, call.CancellationToken)));

    public override Task<Rpc.ActivityPolicyResponse> GetActivityPolicy(
        Rpc.GetActivityPolicyRequest request, ServerCallContext context) =>
        Run(context, request.CourseRunId,
            async (call, principal) =>
            {
                var view = await reads.GetActivityPolicyAsync(principal, request.CourseRunId, request.ActivityId,
                    call.CancellationToken);
                return ToProto(view, OpaqueConcurrencyToken.ToETag(view.UpdateDate));
            });

    public override Task<Rpc.AttemptResponse> GetAttempt(Rpc.GetAttemptRequest request, ServerCallContext context) =>
        Run(context, request.Id, async (call, principal) =>
        {
            var view = await reads.GetAttemptAsync(principal, request.Id, call.CancellationToken);
            return ToProto(view, OpaqueConcurrencyToken.ToETag(view.UpdateDate));
        });

    public override Task<Rpc.AttemptListResponse> ListAttempts(
        Rpc.ListAttemptsRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
            ToProto(await reads.ListAttemptsAsync(principal, request.CourseRunId,
                request.HasActivityId ? request.ActivityId : null, request.HasState ? request.State : null,
                request.HasLimit ? request.Limit : 50, request.HasCursor ? request.Cursor : null,
                call.CancellationToken)));

    // routeId is the {id} route value the HTTP filter read to report a version conflict.
    private Task<T> Run<T>(ServerCallContext context, int? routeId,
        Func<SchoolBotRpcCall, AssessmentPrincipalContext, Task<T>> body) =>
        runner.RunAsync(context, async call =>
        {
            try
            {
                var principal = await authResolver.ResolveAsync(call.Identity, call.CancellationToken);
                return await body(call, principal);
            }
            catch (DomainRuleException error) when (error.Code == ErrorCodes.VersionConflict && routeId is { } id)
            {
                var bank = await repository.GetQuestionBankByIdAsync(id, call.CancellationToken);
                throw new SchoolBotRpcFailure(StatusCodes.Status412PreconditionFailed, ErrorCodes.VersionConflict,
                    "Concurrency token mismatch.", OpaqueConcurrencyToken.ToETag(bank?.UpdateDate));
            }
            catch (System.Text.Json.JsonException)
            {
                throw new SchoolBotRpcFailure(400, ErrorCodes.InvalidInput, "Invalid request body.");
            }
        });

    private static void RejectBody(Rpc.AssessmentBody body)
    {
        switch (body)
        {
            case Rpc.AssessmentBody.NotJson:
                throw new SchoolBotRpcFailure(415, "UNSUPPORTED_MEDIA_TYPE", "Content-Type must be application/json.");
            case Rpc.AssessmentBody.Malformed:
                throw new SchoolBotRpcFailure(400, ErrorCodes.InvalidInput, "Invalid request body.");
            case Rpc.AssessmentBody.DuplicateProperty:
                throw new SchoolBotRpcFailure(400, ErrorCodes.InvalidInput, "Duplicate JSON property.");
            case Rpc.AssessmentBody.Null:
                throw new SchoolBotRpcFailure(400, ErrorCodes.InvalidInput, "A request body is required.");
        }
    }

    // Header absent: no key; anything but one canonical non-empty UUID is rejected.
    private static Guid? RequestKey(IReadOnlyList<string> values)
    {
        if (values.Count == 0) return null;
        if (values.Count != 1 || values[0] is not { Length: 36 } text ||
            !Guid.TryParseExact(text, "D", out var key) || key == Guid.Empty)
            throw new SchoolBotRpcFailure(400, ErrorCodes.InvalidInput, "Invalid idempotency key.");
        return key;
    }

    private static Rpc.QuestionBankMutationResponse ToMutation(QuestionBankMutationResponse mutation)
    {
        var result = new Rpc.QuestionBankMutationResponse
        {
            Id = mutation.Id,
            UpdateDate = Time(mutation.UpdateDate),
            Etag = OpaqueConcurrencyToken.ToETag(mutation.UpdateDate)
        };
        if (Time(mutation.PublishedAt) is { } published) result.PublishedAt = published;
        return result;
    }
}
