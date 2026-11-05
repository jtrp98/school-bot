using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using ServiceSchoolBot.Infrastructure.Extensions;

namespace ServiceSchoolBot.Tests;

internal static class TestRepositoryRegistration
{
    public static IWebHostBuilder UseSchoolBotMemoryStorage(this IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("SchoolBot__Storage", "Memory");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        return builder.UseEnvironment("Development").ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["SchoolBot:Storage"] = "Memory" }))
            .ConfigureServices((context, services) => services.AddInMemoryInfrastructureServices(context.Configuration));
    }

    public static IServiceCollection AddSchoolBotRepositories(
        this IServiceCollection services, ISchoolBotRepositoryScope scope) =>
        services.AddSchoolBotRepositories(_ => scope);

    public static IServiceCollection AddSchoolBotRepositories(
        this IServiceCollection services, Func<IServiceProvider, ISchoolBotRepositoryScope> factory)
    {
        services.AddSingleton(factory);
        services.AddSingleton<IProjectRepository>(provider => provider.GetRequiredService<ISchoolBotRepositoryScope>());
        services.AddSingleton<ILicenseRepository>(provider => provider.GetRequiredService<ISchoolBotRepositoryScope>());
        services.AddSingleton<ICurriculumRepository>(provider => provider.GetRequiredService<ISchoolBotRepositoryScope>());
        services.AddSingleton<ICurriculumImportRepository>(provider => provider.GetRequiredService<ISchoolBotRepositoryScope>());
        services.AddSingleton<IAssessmentRepository>(provider => provider.GetRequiredService<ISchoolBotRepositoryScope>());
        return services;
    }

    public static IServiceCollection AddTransientSchoolBotRepositories(
        this IServiceCollection services, Func<IServiceProvider, ISchoolBotRepositoryScope> factory)
    {
        services.AddTransient(factory);
        services.AddTransient<IProjectRepository>(provider => provider.GetRequiredService<ISchoolBotRepositoryScope>());
        services.AddTransient<ILicenseRepository>(provider => provider.GetRequiredService<ISchoolBotRepositoryScope>());
        services.AddTransient<ICurriculumRepository>(provider => provider.GetRequiredService<ISchoolBotRepositoryScope>());
        services.AddTransient<ICurriculumImportRepository>(provider => provider.GetRequiredService<ISchoolBotRepositoryScope>());
        services.AddTransient<IAssessmentRepository>(provider => provider.GetRequiredService<ISchoolBotRepositoryScope>());
        return services;
    }
}

