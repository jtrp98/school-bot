using Grpc.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.Tests;

public sealed class FakeClock(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset now = start;
    public override DateTimeOffset GetUtcNow() => now;
    public void Advance(TimeSpan amount) => now += amount;
}

// SchoolBot host reached over gRPC as the Gateway; stub principals stand in for authenticated callers
// unless a test needs the real Gateway identity resolver.
public sealed class ApiFactory(bool stubPrincipals = true) : WebApplicationFactory<Program>
{
    public FakeClock Clock { get; } = new(DateTimeOffset.UtcNow);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSchoolBotMemoryStorage();
        builder.ConfigureServices(services => services.AddSingleton<TimeProvider>(Clock));
        builder.ConfigureTestServices(services =>
        {
            services.AddTrustedGateway();
            if (stubPrincipals) services.AddStubPrincipals();
        });
    }
}

internal static class ApiHelpers
{
    public const string Sha256 = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    public static Metadata Admin() => GrpcTest.Headers(GatewayCaller.CentralAdmin());

    public static Metadata AdminV2(string token = LicenseAdminPrincipalStub.ValidAdminToken) => GrpcTest.As(token);

    public static Rpc.CreateReleaseRequest ReleaseFields(string version = "1.0.0", string channel = "stable",
        string platform = "windows-amd64") => new()
    {
        Version = version,
        Channel = channel,
        Platform = platform,
        Url = "https://example.com/school-bot.zip",
        Sha256 = Sha256,
        Signature = "metadata-only-test"
    };

    public static async Task<string> CreateReleaseId(Rpc.ReleaseService.ReleaseServiceClient releases,
        Rpc.CreateReleaseRequest fields) =>
        (await releases.CreateReleaseAsync(fields, Admin())).Id;

    public static async Task Publish(Rpc.ReleaseService.ReleaseServiceClient releases, string releaseId) =>
        await releases.PublishReleaseAsync(new Rpc.PublishReleaseRequest { Id = releaseId }, Admin());
}
