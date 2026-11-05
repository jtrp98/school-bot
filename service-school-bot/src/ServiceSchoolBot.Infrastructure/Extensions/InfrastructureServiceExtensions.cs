using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Infrastructure.Configuration;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using ServiceSchoolBot.Infrastructure.Persistence;
using Memory = ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;
using ServiceSchoolBot.Application.Common.Model.Assessment;

namespace ServiceSchoolBot.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<IDatabaseConnectionConfig>(_ => new DatabaseConnectionConfig(key => configuration[key]));
        services.AddDatabaseContexts();
        services.AddScoped<SchoolBotUnitOfWork>();
        services.AddScoped<ISchoolBotRepositoryScope>(provider => provider.GetRequiredService<SchoolBotUnitOfWork>());
        services.AddScoped<IProjectRepository>(provider => provider.GetRequiredService<SchoolBotUnitOfWork>().Project);
        services.AddScoped<ILicenseRepository>(provider => provider.GetRequiredService<SchoolBotUnitOfWork>().License);
        services.AddScoped<IWebSessionRepository>(provider => provider.GetRequiredService<SchoolBotUnitOfWork>().WebSession);
        services.AddScoped<ICurriculumRepository>(provider => provider.GetRequiredService<SchoolBotUnitOfWork>().Curriculum);
        services.AddScoped<ICurriculumImportRepository>(provider => provider.GetRequiredService<SchoolBotUnitOfWork>().CurriculumImport);
        services.AddScoped<IAssessmentRepository>(provider => provider.GetRequiredService<SchoolBotUnitOfWork>().Assessment);
        services.AddScoped<IIdempotencyRepository>(provider => provider.GetRequiredService<SchoolBotUnitOfWork>().Idempotency);
        services.AddScoped<IMasterAccountRepository, MasterAccountRepository>();
        services.AddScoped<ISchoolProfileRepository, SchoolProfileRepository>();
        services.AddScoped<ILogsRepository, LogsRepository>();
        services.AddScoped<IGradeRepository, GradeRepository>();
        services.AddScoped<IAssessmentAuthority, SchoolAssessmentAuthority>();
        services.AddScoped<IGradeDestination, PostgresGradeDestination>();
        services.AddScoped<IReleaseRepository, ReleaseRepository>();
        return services;
    }

    public static IServiceCollection AddInMemoryInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<IDatabaseConnectionConfig>(_ => new DatabaseConnectionConfig(key => configuration[key]));
        services.AddDatabaseContexts();
        services.AddSingleton<ISchoolBotBackingStore, MemorySchoolBotBackingStore>();
        services.AddSingleton<Memory.SchoolBotUnitOfWork>();
        services.AddSingleton<ISchoolBotRepositoryScope>(provider => provider.GetRequiredService<Memory.SchoolBotUnitOfWork>());
        services.AddSingleton<IProjectRepository>(provider => provider.GetRequiredService<Memory.SchoolBotUnitOfWork>().Project);
        services.AddSingleton<ILicenseRepository>(provider => provider.GetRequiredService<Memory.SchoolBotUnitOfWork>().License);
        services.AddSingleton<IWebSessionRepository>(provider => provider.GetRequiredService<Memory.SchoolBotUnitOfWork>().WebSession);
        services.AddSingleton<ICurriculumRepository>(provider => provider.GetRequiredService<Memory.SchoolBotUnitOfWork>().Curriculum);
        services.AddSingleton<ICurriculumImportRepository>(provider => provider.GetRequiredService<Memory.SchoolBotUnitOfWork>().CurriculumImport);
        services.AddSingleton<IAssessmentRepository>(provider => provider.GetRequiredService<Memory.SchoolBotUnitOfWork>().Assessment);
        services.AddSingleton<IIdempotencyRepository>(provider => provider.GetRequiredService<Memory.SchoolBotUnitOfWork>().Idempotency);
        services.AddSingleton<IMasterAccountRepository, Memory.MasterAccountRepository>();
        services.AddSingleton<ISchoolProfileRepository, Memory.SchoolProfileRepository>();
        services.AddSingleton<ILogsRepository, Memory.LogsRepository>();
        services.AddSingleton<IGradeRepository, Memory.GradeRepository>();
        services.AddSingleton<IReleaseRepository, Memory.ReleaseRepository>();
        return services;
    }

    public static IServiceCollection AddManagementStore(this IServiceCollection services)
    {
        services.AddSingleton<IManagementStore, InMemoryManagementStore>();
        return services;
    }
}
