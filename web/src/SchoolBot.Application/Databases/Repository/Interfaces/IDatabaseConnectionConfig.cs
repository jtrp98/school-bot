namespace SchoolBot.Application.Databases.Repository.Interfaces;

public interface IDatabaseConnectionConfig
{
    string? SchoolBotDbConnectionString { get; }
    string? MasterDbConnectionString { get; }
    string? SchoolDbConnectionString { get; }
    string? LogsDbConnectionString { get; }
    string? PGGradeDbConnectionString { get; }

    bool IsSchoolBotDbConfigured => !string.IsNullOrWhiteSpace(SchoolBotDbConnectionString);
    bool IsMasterDbConfigured => !string.IsNullOrWhiteSpace(MasterDbConnectionString);
    bool IsSchoolDbConfigured => !string.IsNullOrWhiteSpace(SchoolDbConnectionString);
    bool IsLogsDbConfigured => !string.IsNullOrWhiteSpace(LogsDbConnectionString);
    bool IsPGGradeDbConfigured => !string.IsNullOrWhiteSpace(PGGradeDbConnectionString);
}
