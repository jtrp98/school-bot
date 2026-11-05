using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceSchoolBot.API.GrpcServices;

namespace ServiceSchoolBot.API.Extensions;

public static class ApiServiceExtensions
{
    // Every caller is authenticated by the Gateway; these turn its identity into service principals.
    public static IServiceCollection AddPrincipalResolvers(this IServiceCollection services)
    {
        services.AddScoped<GatewayPrincipalResolver>();
        services.AddScoped<IProjectAuthResolver>(provider => provider.GetRequiredService<GatewayPrincipalResolver>());
        services.AddScoped<ICurriculumAuthResolver>(provider => provider.GetRequiredService<GatewayPrincipalResolver>());
        services.AddScoped<ILicenseAdminAuthResolver>(provider => provider.GetRequiredService<GatewayPrincipalResolver>());
        services.AddScoped<IAssessmentAuthResolver, GatewayAssessmentAuthResolver>();
        services.Replace(ServiceDescriptor.Scoped<ServiceSchoolBot.Application.Common.Model.Assessment.IExportPrincipalResolver, GatewayExportPrincipalResolver>());
        return services;
    }
}
