using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ServiceSchoolBot.Application.Common.Model.Projects;
using ServiceSchoolBot.Application.Services.Implementations.Projects;
using ServiceSchoolBot.Application.Services.Interfaces.Projects;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.API.GrpcServices;

public sealed class ProjectGrpcService(
    SchoolBotGrpcRunner runner,
    IProjectService service,
    IProjectAuthResolver authResolver) : Rpc.ProjectService.ProjectServiceBase
{
    public override Task<Rpc.ListProjectsResponse> ListProjects(
        Rpc.ListProjectsRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var auth = await AuthorizeAsync(call);
            var list = await service.ListProjectsAsync(auth, request.HasMode ? request.Mode : null,
                request.HasLimit ? request.Limit : 50, request.HasCursor ? request.Cursor : null, call.CancellationToken);
            var response = new Rpc.ListProjectsResponse();
            if (list.NextCursor is not null) response.NextCursor = list.NextCursor;
            response.Items.AddRange(list.Items.Select(item => new Rpc.ProjectItem
            {
                Id = item.Id,
                ProjectId = item.ProjectId,
                Mode = item.Mode,
                Name = item.Name,
                Revision = item.Revision,
                UpdateDate = Timestamp.FromDateTimeOffset(item.UpdateDate)
            }));
            return response;
        });

    public override Task<Rpc.ProjectDocumentResponse> CreateProject(
        Rpc.CreateProjectRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var auth = await AuthorizeAsync(call);
            var document = await service.CreateProjectAsync(auth,
                new CreateProjectRequest(request.Mode, request.Name, ParseContent(request.ContentJson)),
                IdempotencyKey(request.HasIdempotencyKey ? request.IdempotencyKey : null), call.CancellationToken);
            return ToResponse(document);
        });

    public override Task<Rpc.ProjectDocumentResponse> GetProject(
        Rpc.GetProjectRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var auth = await AuthorizeAsync(call);
            return ToResponse(await service.GetProjectAsync(auth, request.Id, call.CancellationToken));
        });

    public override Task<Rpc.ProjectDocumentResponse> UpdateProject(
        Rpc.UpdateProjectRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var auth = await AuthorizeAsync(call);
            var ifMatch = RequireIfMatch(request.HasIfMatch ? request.IfMatch : null);
            var document = await service.UpdateProjectAsync(auth, request.Id,
                new UpdateProjectRequest(request.Name, ParseContent(request.ContentJson)), ifMatch, call.CancellationToken);
            return ToResponse(document);
        });

    public override Task<Rpc.DeleteProjectResponse> DeleteProject(
        Rpc.DeleteProjectRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var auth = await AuthorizeAsync(call);
            var ifMatch = RequireIfMatch(request.HasIfMatch ? request.IfMatch : null);
            var deleted = await service.DeleteProjectAsync(auth, request.Id, ifMatch, call.CancellationToken);
            return new Rpc.DeleteProjectResponse
            {
                ProjectId = deleted.ProjectId,
                Deleted = deleted.Deleted,
                UpdateDate = Timestamp.FromDateTimeOffset(deleted.UpdateDate),
                Etag = OpaqueConcurrencyToken.ToETag(deleted.UpdateDate)
            };
        });

    public override Task<Rpc.ProjectDocumentResponse> CopyProject(
        Rpc.CopyProjectRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var auth = await AuthorizeAsync(call);
            var ifMatch = RequireIfMatch(request.HasIfMatch ? request.IfMatch : null);
            var document = await service.CopyProjectAsync(auth, request.Id,
                new CopyProjectRequest(request.Name, request.SourceProjectRevisionId), ifMatch,
                IdempotencyKey(request.HasIdempotencyKey ? request.IdempotencyKey : null), call.CancellationToken);
            return ToResponse(document);
        });

    private async Task<ProjectAuthContext> AuthorizeAsync(SchoolBotRpcCall call)
    {
        var auth = await authResolver.ResolveAsync(call.Identity, call.CancellationToken);
        var check = ProjectAuthorizationService.Evaluate(auth);
        if (!check.IsAllowed) throw new SchoolBotRpcFailure(check.StatusCode, check.ErrorCode!, check.Message!);
        return auth;
    }

    private static string RequireIfMatch(string? ifMatch) => string.IsNullOrWhiteSpace(ifMatch)
        ? throw new SchoolBotRpcFailure(StatusCodes.Status428PreconditionRequired,
            ErrorCodes.PreconditionRequired, "If-Match header is required.")
        : ifMatch;

    // Same rule as the HTTP header: an unparseable key is ignored rather than rejected.
    private static Guid? IdempotencyKey(string? value) =>
        !string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var key) ? key : null;

    // The Gateway already parsed this JSON; a failure here means a broken caller, not a client error.
    private static JsonElement ParseContent(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static Rpc.ProjectDocumentResponse ToResponse(CloudProjectDocumentResponse document) => new()
    {
        Document = new Rpc.ProjectDocumentItem
        {
            Schema = document.Schema,
            ProjectId = document.ProjectId,
            ProjectRevisionId = document.ProjectRevisionId,
            Mode = document.Mode,
            Name = document.Name,
            Revision = document.Revision,
            UpdateDate = Timestamp.FromDateTimeOffset(document.UpdateDate),
            ContentJson = document.Content.GetRawText()
        },
        Etag = OpaqueConcurrencyToken.ToETag(document.UpdateDate)
    };
}
