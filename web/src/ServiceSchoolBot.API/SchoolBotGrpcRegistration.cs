using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceSchoolBot.API.GrpcServices;

namespace ServiceSchoolBot.API;

public static class SchoolBotGrpcRegistration
{
    public static IServiceCollection AddSchoolBotGrpc(
        this IServiceCollection services,
        Action<SchoolBotGrpcOptions>? configureOptions = null)
    {
        services.AddGrpc(options =>
        {
            options.MaxReceiveMessageSize = 4 * 1024 * 1024;
            options.MaxSendMessageSize = 4 * 1024 * 1024;
        });

        services.TryAddSingleton<ITrustedGatewayPeerVerifier, DenyAllGatewayPeerVerifier>();
        services.AddSingleton<SchoolBotGrpcRunner>();

        var options = new SchoolBotGrpcOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        return services;
    }

    public static IEndpointRouteBuilder MapSchoolBotGrpc(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<ProjectGrpcService>();
        endpoints.MapGrpcService<LicenseGrpcService>();
        endpoints.MapGrpcService<CurriculumGrpcService>();
        endpoints.MapGrpcService<AssessmentGrpcService>();
        endpoints.MapGrpcService<ReleaseGrpcService>();
        return endpoints;
    }
}
