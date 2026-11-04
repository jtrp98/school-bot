using Grpc.Core;
using ServiceSchoolBot.Application.Common.Model.Management;
using ServiceSchoolBot.Application.Services.Implementations.Management;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Releases;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.API.GrpcServices;

// Desktop update checks need no sign-in; managing releases is for central staff (TAdmin) only.
public sealed class ReleaseGrpcService(SchoolBotGrpcRunner runner, ManagementService service)
    : Rpc.ReleaseService.ReleaseServiceBase
{
    public override Task<Rpc.CheckUpdateResponse> CheckUpdate(Rpc.CheckUpdateRequest request, ServerCallContext context) =>
        runner.RunAsync(context, _ =>
        {
            var release = service.CheckUpdate(request.Version, request.Channel, request.Platform);
            var response = new Rpc.CheckUpdateResponse();
            if (release is not null) response.Release = ToProto(release);
            return Task.FromResult(response);
        });

    public override Task<Rpc.ListReleasesResponse> ListReleases(Rpc.ListReleasesRequest request, ServerCallContext context) =>
        runner.RunAsync(context, call =>
        {
            RequireStaff(call);
            var response = new Rpc.ListReleasesResponse();
            response.Items.AddRange(service.ListReleases().Select(ToProto));
            return Task.FromResult(response);
        });

    public override Task<Rpc.ReleaseItem> CreateRelease(Rpc.CreateReleaseRequest request, ServerCallContext context) =>
        runner.RunAsync(context, call =>
        {
            RequireStaff(call);
            return Task.FromResult(ToProto(service.CreateRelease(new CreateReleaseRequest(request.Version,
                request.Channel, request.Platform, request.Url, request.Sha256, request.Signature))));
        });

    public override Task<Rpc.ReleaseItem> PublishRelease(Rpc.PublishReleaseRequest request, ServerCallContext context) =>
        runner.RunAsync(context, call =>
        {
            RequireStaff(call);
            return Task.FromResult(ToProto(service.PublishRelease(request.Id)));
        });

    private static void RequireStaff(SchoolBotRpcCall call)
    {
        if (call.Identity?.AdminId is null)
            throw new SchoolBotRpcFailure(StatusCodes.Status403Forbidden, ErrorCodes.StaffRoleRequired,
                "Central staff access is required.");
    }

    private static Rpc.ReleaseItem ToProto(DesktopRelease release) => new()
    {
        Id = release.Id,
        Version = release.Version,
        Channel = release.Channel,
        Platform = release.Platform,
        Url = release.Url,
        Sha256 = release.Sha256,
        Signature = release.Signature,
        Published = release.Published
    };
}
