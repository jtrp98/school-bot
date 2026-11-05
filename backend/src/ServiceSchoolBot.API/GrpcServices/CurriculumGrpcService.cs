using System.Text.Json;
using Grpc.Core;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Interfaces.Curriculum;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using static ServiceSchoolBot.API.GrpcServices.CurriculumProtoMapper;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.API.GrpcServices;

public sealed class CurriculumGrpcService(
    SchoolBotGrpcRunner runner,
    ICurriculumService service,
    ICurriculumAuthResolver authResolver,
    ICurriculumRepository curricula) : Rpc.CurriculumService.CurriculumServiceBase
{
    public override Task<Rpc.CurriculumListResponse> ListPublishedCurricula(
        Rpc.ListPublishedCurriculaRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var principal = await ResolveAsync(call);
            return await Handled(async () => ToProto(await service.ListPublishedCurriculaAsync(principal,
                request.HasLevelId ? request.LevelId : null,
                request.HasRecommendedGrade ? request.RecommendedGrade : null,
                request.HasSubjectKey ? Guid.Parse(request.SubjectKey) : null,
                request.HasMode ? request.Mode : null,
                request.HasLimit ? request.Limit : 50,
                request.HasCursor ? request.Cursor : null,
                call.CancellationToken)));
        });

    public override Task<Rpc.CurriculumDraftResponse> GetPublishedCurriculum(
        Rpc.GetCurriculumRequest request, ServerCallContext context) =>
        runner.RunAsync(context, call => Handled(async () =>
        {
            var principal = await authResolver.ResolveAsync(call.Identity, call.CancellationToken);
            return ToProto(await service.GetPublishedViewAsync(principal, request.Id, call.CancellationToken));
        }));

    public override Task<Rpc.CurriculumListResponse> ListAdminCurricula(
        Rpc.ListAdminCurriculaRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var principal = await ResolveAsync(call);
            return await Handled(async () => ToProto(await service.ListAdminCurriculaAsync(principal,
                request.HasLifecycle ? request.Lifecycle : null,
                request.HasLevelId ? request.LevelId : null,
                request.HasLimit ? request.Limit : 50,
                request.HasCursor ? request.Cursor : null,
                call.CancellationToken)));
        });

    public override Task<Rpc.CurriculumAdminResponse> GetAdminCurriculum(
        Rpc.GetCurriculumRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var principal = await ResolveAsync(call);
            try
            {
                return await Handled(async () =>
                    ToProto(await service.GetDraftViewAsync(principal, request.Id, call.CancellationToken)));
            }
            catch (JsonException)
            {
                throw new SchoolBotRpcFailure(StatusCodes.Status500InternalServerError, "HTTP_ERROR",
                    "The request failed.");
            }
        });

    public override Task<Rpc.CurriculumMutationResponse> CreateCurriculum(
        Rpc.CreateCurriculumRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var principal = await ResolveAsync(call);
            var body = new CreateCurriculumRequest(Guid.Parse(request.CurriculumKey), request.Revision,
                request.LevelId, request.Title, request.SourceRef, request.RecommendedGrades.ToList(),
                request.Subjects.Select(ToDomain).ToList());
            return await Handled(async () => ToProto(await service.CreateCurriculumAsync(principal, body,
                call.RequestId, IdempotencyKey(request.HasIdempotencyKey ? request.IdempotencyKey : null),
                call.CancellationToken)));
        });

    public override Task<Rpc.CurriculumMutationResponse> UpdateCurriculumMetadata(
        Rpc.UpdateCurriculumMetadataRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var principal = await ResolveAsync(call);
            var body = new CurriculumMetadataInput(request.LevelId, request.Title, request.SourceRef,
                request.RecommendedGrades.ToList(), request.Subjects.Select(ToDomain).ToList());
            return await MutateAsync(call, request.Id, () => service.UpdateMetadataAsync(principal, request.Id, body,
                request.HasIfMatch ? request.IfMatch : "", call.RequestId,
                IdempotencyKey(request.HasIdempotencyKey ? request.IdempotencyKey : null), call.CancellationToken));
        });

    public override Task<Rpc.CurriculumMutationResponse> UpdateCurriculumContent(
        Rpc.UpdateCurriculumContentRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var principal = await ResolveAsync(call);
            var body = new UpdateCurriculumContentRequest(request.Lessons.Select(ToDomain).ToList());
            return await MutateAsync(call, request.Id, () => service.UpdateContentAsync(principal, request.Id, body,
                request.HasIfMatch ? request.IfMatch : "", call.RequestId,
                IdempotencyKey(request.HasIdempotencyKey ? request.IdempotencyKey : null), call.CancellationToken));
        });

    public override Task<Rpc.CurriculumMutationResponse> PublishCurriculum(
        Rpc.PublishCurriculumRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var principal = await ResolveAsync(call);
            var body = new PublishCurriculumRequest(request.HasApprovalRef ? request.ApprovalRef : null);
            return await MutateAsync(call, request.Id, () => service.PublishCurriculumAsync(principal, request.Id, body,
                request.HasIfMatch ? request.IfMatch : "", call.RequestId,
                IdempotencyKey(request.HasIdempotencyKey ? request.IdempotencyKey : null), call.CancellationToken));
        });

    public override Task<Rpc.CurriculumImportResponse> ImportCurriculum(
        Rpc.ImportCurriculumRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var principal = await ResolveAsync(call);
            Guid? key = null;
            if (request.HasIdempotencyKey && !string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                if (!Guid.TryParse(request.IdempotencyKey, out var parsed) || parsed == Guid.Empty)
                    throw new SchoolBotRpcFailure(400, ErrorCodes.InvalidInput, "Invalid Idempotency-Key.");
                key = parsed;
            }

            if (request.Body == Rpc.CurriculumImportBody.NotJson)
                throw new SchoolBotRpcFailure(415, "UNSUPPORTED_MEDIA_TYPE", "Content-Type must be application/json.");
            if (request.Body == Rpc.CurriculumImportBody.Malformed)
                throw new SchoolBotRpcFailure(400, ErrorCodes.InvalidInput, "Invalid curriculum import input.");

            var body = new CurriculumImportRequest(request.SourceRef, request.Rows.Select(row =>
                new CurriculumImportRow(Guid.Parse(row.SourceRowKey), row.SourcePosition,
                    row.HasSourceApproved ? row.SourceApproved : null, ToDomain(row.Lesson))).ToList());
            try
            {
                return ToProto(await service.ImportCurriculumLessonsAsync(principal, request.Id, body, call.RequestId, key,
                    call.CancellationToken));
            }
            catch (DomainRuleException error)
            {
                throw new SchoolBotRpcFailure(ErrorStatus.For(error.Code), error.Code, error.Message,
                    details: ImportDetails(error.Data["details"]));
            }
        });

    private async Task<CurriculumPrincipalContext> ResolveAsync(SchoolBotRpcCall call)
    {
        try
        {
            return await authResolver.ResolveAsync(call.Identity, call.CancellationToken);
        }
        catch (DomainRuleException error)
        {
            throw SchoolBotRpcFailure.From(error);
        }
    }

    private static async Task<T> Handled<T>(Func<Task<T>> body)
    {
        try
        {
            return await body();
        }
        catch (DomainRuleException error)
        {
            throw SchoolBotRpcFailure.From(error);
        }
    }

    // A version conflict reports the curriculum's current ETag, read after the failed commit.
    private async Task<Rpc.CurriculumMutationResponse> MutateAsync(
        SchoolBotRpcCall call, int id, Func<Task<CurriculumMutationResponse>> mutate)
    {
        try
        {
            return ToProto(await mutate());
        }
        catch (DomainRuleException error) when (error.Code == ErrorCodes.VersionConflict)
        {
            var current = await curricula.GetCurriculumByIdAsync(id, call.CancellationToken);
            throw new SchoolBotRpcFailure(StatusCodes.Status412PreconditionFailed, ErrorCodes.VersionConflict,
                "Concurrency token mismatch.", OpaqueConcurrencyToken.ToETag(current?.UpdateDate));
        }
        catch (DomainRuleException error)
        {
            throw SchoolBotRpcFailure.From(error);
        }
    }

    private static Guid? IdempotencyKey(string? value) =>
        !string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var key) ? key : null;

    private static Rpc.ErrorDetails? ImportDetails(object? details)
    {
        if (details is null) return null;
        var element = JsonSerializer.SerializeToElement(details);
        var result = new Rpc.ErrorDetails();
        if (element.TryGetProperty("field", out var field))
        {
            result.ImportRow = true;
            result.Field = field.GetString()!;
            if (element.GetProperty("source_position").ValueKind == JsonValueKind.Number)
                result.SourcePosition = element.GetProperty("source_position").GetInt32();
        }

        if (element.TryGetProperty("source_row_key", out var rowKey) && rowKey.ValueKind == JsonValueKind.String)
            result.SourceRowKey = rowKey.GetString()!;
        return result;
    }
}
