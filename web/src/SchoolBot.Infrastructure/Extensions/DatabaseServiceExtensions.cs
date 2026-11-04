using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolBot.Application.Databases.Repository.Interfaces;
using SchoolBot.Infrastructure.Common.Databases;

namespace SchoolBot.Infrastructure.Extensions;

public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddDatabaseContexts(this IServiceCollection services)
    {
        services.AddDbContext<SchoolBotDbContext>((provider, options) =>
            Configure(options, provider.GetRequiredService<IDatabaseConnectionConfig>().SchoolBotDbConnectionString,
                Configuration.DatabaseConnectionConfig.SchoolBotDbEnvVar));
        services.AddDbContext<JabjaiMasterSingleDBPostgreSQLDBContext>((provider, options) =>
            Configure(options, provider.GetRequiredService<IDatabaseConnectionConfig>().MasterDbConnectionString,
                Configuration.DatabaseConnectionConfig.MasterDbEnvVar));
        services.AddDbContext<JabjaiSchoolSingleDBPostgreSQLDBContext>((provider, options) =>
            Configure(options, provider.GetRequiredService<IDatabaseConnectionConfig>().SchoolDbConnectionString,
                Configuration.DatabaseConnectionConfig.SchoolDbEnvVar));
        services.AddDbContext<JabjaiSchoolLogsPostgreSQLDBContext>((provider, options) =>
            Configure(options, provider.GetRequiredService<IDatabaseConnectionConfig>().LogsDbConnectionString,
                Configuration.DatabaseConnectionConfig.LogsDbEnvVar));
        services.AddDbContext<PGGradeDBPostgreSQLDBContext>((provider, options) =>
            Configure(options, provider.GetRequiredService<IDatabaseConnectionConfig>().PGGradeDbConnectionString,
                Configuration.DatabaseConnectionConfig.PGGradeDbEnvVar));
        return services;
    }

    private static void Configure(DbContextOptionsBuilder options, string? connectionString, string setting)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Missing database configuration: {setting}");
        try
        {
            var connection = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
            connection.IncludeErrorDetail = false;
            connection.LogParameters = false;
            options.UseNpgsql(connection.ConnectionString);
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException($"Invalid database configuration: {setting}");
        }
    }
}
