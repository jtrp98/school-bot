using SchoolBot.Application.Databases.Repository.Interfaces;

namespace SchoolBot.Infrastructure.Configuration;

public sealed class DatabaseConnectionConfig : IDatabaseConnectionConfig
{
    public const string SchoolBotDbEnvVar = "SCHOOL_BOT_DB_CONNECTION_STRING";
    public const string MasterDbEnvVar = "MASTER_DB_CONNECTION_STRING";
    public const string SchoolDbEnvVar = "SCHOOL_DB_CONNECTION_STRING";
    public const string LogsDbEnvVar = "LOGS_DB_CONNECTION_STRING";
    public const string PGGradeDbEnvVar = "PGGRADE_DB_CONNECTION_STRING";

    private readonly Func<string, string?>? configLookup;

    public DatabaseConnectionConfig(Func<string, string?>? configLookup = null)
    {
        this.configLookup = configLookup;
    }

    public string? SchoolBotDbConnectionString => GetConfigValue(SchoolBotDbEnvVar, "ConnectionStrings:SchoolBotDb");
    public string? MasterDbConnectionString => GetConfigValue(MasterDbEnvVar, "ConnectionStrings:MasterDb");
    public string? SchoolDbConnectionString => GetConfigValue(SchoolDbEnvVar, "ConnectionStrings:SchoolDb");
    public string? LogsDbConnectionString => GetConfigValue(LogsDbEnvVar, "ConnectionStrings:LogsDb");
    public string? PGGradeDbConnectionString => GetConfigValue(PGGradeDbEnvVar, "ConnectionStrings:PGGradeDb");

    public bool IsSchoolBotDbConfigured => !string.IsNullOrWhiteSpace(SchoolBotDbConnectionString);
    public bool IsMasterDbConfigured => !string.IsNullOrWhiteSpace(MasterDbConnectionString);
    public bool IsSchoolDbConfigured => !string.IsNullOrWhiteSpace(SchoolDbConnectionString);
    public bool IsLogsDbConfigured => !string.IsNullOrWhiteSpace(LogsDbConnectionString);
    public bool IsPGGradeDbConfigured => !string.IsNullOrWhiteSpace(PGGradeDbConnectionString);

    private string? GetConfigValue(string envName, string configKey)
    {
        var fromEnv = Environment.GetEnvironmentVariable(envName);
        if (!string.IsNullOrWhiteSpace(fromEnv)) return fromEnv.Trim();
        var fromLookup = configLookup?.Invoke(configKey) ?? configLookup?.Invoke(envName);
        return string.IsNullOrWhiteSpace(fromLookup) ? null : fromLookup.Trim();
    }
}
