using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolBot.Application.Databases.Repository.Interfaces;
using SchoolBot.Infrastructure.Common.Databases;
using SchoolBot.Infrastructure.Configuration;
using SchoolBot.Infrastructure.Extensions;
using SchoolBot.Infrastructure.Repositories.Storage;

const string syntheticConnection = "Host=localhost;Database=self_check;Username=self_check;Password=synthetic_only";
var variables = new[]
{
    DatabaseConnectionConfig.SchoolBotDbEnvVar,
    DatabaseConnectionConfig.MasterDbEnvVar,
    DatabaseConnectionConfig.SchoolDbEnvVar,
    DatabaseConnectionConfig.LogsDbEnvVar,
    DatabaseConnectionConfig.PGGradeDbEnvVar
};
var originalEnvironment = variables.ToDictionary(key => key, Environment.GetEnvironmentVariable);
try
{
    foreach (var variable in variables) Environment.SetEnvironmentVariable(variable, null);
    var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["ConnectionStrings:SchoolBotDb"] = syntheticConnection,
        ["ConnectionStrings:MasterDb"] = syntheticConnection,
        ["ConnectionStrings:SchoolDb"] = syntheticConnection,
        ["ConnectionStrings:LogsDb"] = syntheticConnection,
        ["ConnectionStrings:PGGradeDb"] = syntheticConnection
    }).Build();
    var config = new DatabaseConnectionConfig(key => configuration[key]);
    Assert(Connections(config).All(value => value == syntheticConnection), "Configuration fallback failed.");
    foreach (var variable in variables) Environment.SetEnvironmentVariable(variable, "  " + syntheticConnection + "  ");
    Assert(Connections(new DatabaseConnectionConfig()).All(value => value == syntheticConnection),
        "Environment lookup failed.");
    var services = new ServiceCollection().AddInfrastructureServices(configuration);
    using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    using var scope = provider.CreateScope();
    CheckModel<SchoolBotDbContext>(scope.ServiceProvider, "SchoolBot");
    var schoolBotContext = scope.ServiceProvider.GetRequiredService<SchoolBotDbContext>();
    CheckSchoolBotModel(schoolBotContext);
    Assert(ReferenceEquals(schoolBotContext, scope.ServiceProvider.GetRequiredService<SchoolBotDbContext>()),
        "SchoolBot context was not reused within its scope.");
    using (var otherScope = provider.CreateScope())
        Assert(!ReferenceEquals(schoolBotContext, otherScope.ServiceProvider.GetRequiredService<SchoolBotDbContext>()),
            "SchoolBot context was shared across scopes.");
    CheckModel<JabjaiMasterSingleDBPostgreSQLDBContext>(scope.ServiceProvider, "Master");
    CheckModel<JabjaiSchoolSingleDBPostgreSQLDBContext>(scope.ServiceProvider, "School");
    CheckModel<JabjaiSchoolLogsPostgreSQLDBContext>(scope.ServiceProvider, "Logs");
    CheckModel<PGGradeDBPostgreSQLDBContext>(scope.ServiceProvider, "PGGrade");
    Assert(provider.GetRequiredService<ISchoolBotBackingStore>() is MemorySchoolBotBackingStore,
        "Business backing store changed.");
    Environment.SetEnvironmentVariable(DatabaseConnectionConfig.MasterDbEnvVar, "invalid=synthetic_only");
    using var invalidScope = provider.CreateScope();
    try
    {
        invalidScope.ServiceProvider.GetRequiredService<JabjaiMasterSingleDBPostgreSQLDBContext>();
        throw new Exception("Invalid connection accepted.");
    }
    catch (InvalidOperationException exception)
    {
        Assert(!exception.ToString().Contains("synthetic_only", StringComparison.Ordinal), "Secret leaked.");
    }
    CheckSchoolBotConfigurationError("invalid=synthetic_only", "Invalid");
    CheckSchoolBotConfigurationError(null, "Missing");
    Console.WriteLine("PASS: configuration, DI, models and unchanged memory repository; no database connection opened.");
}
finally
{
    foreach (var (key, value) in originalEnvironment) Environment.SetEnvironmentVariable(key, value);
}

static string?[] Connections(IDatabaseConnectionConfig config) =>
[
    config.SchoolBotDbConnectionString, config.MasterDbConnectionString, config.SchoolDbConnectionString,
    config.LogsDbConnectionString, config.PGGradeDbConnectionString
];

static void CheckModel<TContext>(IServiceProvider provider, string database) where TContext : DbContext
{
    var context = provider.GetRequiredService<TContext>();
    Assert(context.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL", "Unexpected provider.");
    Assert(context.Database.GetDbConnection().State == System.Data.ConnectionState.Closed, "Connection opened.");
    Assert(!context.GetService<IDbContextOptions>().Extensions.OfType<CoreOptionsExtension>()
        .Any(extension => extension.IsSensitiveDataLoggingEnabled), "Sensitive logging enabled.");
    var count = context.Model.GetEntityTypes().Count();
    Assert(count > 0, "Empty model.");
    Console.WriteLine($"{database}: 1 context, {count} mapped entities.");
}

static void CheckSchoolBotModel(SchoolBotDbContext context)
{
    var expectedProperties = new Dictionary<string, int>
    {
        ["SchoolBotActivity"] = 15,
        ["SchoolBotAssessment"] = 18,
        ["SchoolBotAttempt"] = 19,
        ["SchoolBotAuditEvent"] = 13,
        ["SchoolBotCourseRun"] = 16,
        ["SchoolBotCourseRunActivity"] = 12,
        ["SchoolBotCriterion"] = 13,
        ["SchoolBotCurriculum"] = 16,
        ["SchoolBotGradeSlot"] = 17,
        ["SchoolBotGradingJob"] = 14,
        ["SchoolBotIdempotency"] = 18,
        ["SchoolBotLesson"] = 13,
        ["SchoolBotLevel"] = 11,
        ["SchoolBotLevelPromotionPolicy"] = 11,
        ["SchoolBotLevelProposal"] = 16,
        ["SchoolBotLicense"] = 15,
        ["SchoolBotLicenseBinding"] = 15,
        ["SchoolBotMission"] = 13,
        ["SchoolBotProject"] = 12,
        ["SchoolBotProjectRevision"] = 11,
        ["SchoolBotQuestionBank"] = 14,
        ["SchoolBotRelease"] = 15,
        ["SchoolBotRetakeGrant"] = 12,
        ["SchoolBotScoreExport"] = 11,
        ["SchoolBotScoreMapping"] = 11,
        ["SchoolBotScoreOutbox"] = 19,
        ["SchoolBotScorePreview"] = 14,
        ["SchoolBotScorePreviewItem"] = 12,
        ["SchoolBotScoringPolicy"] = 10,
        ["SchoolBotSession"] = 17,
        ["SchoolBotStaffRole"] = 12,
        ["SchoolBotSubmission"] = 15
    };
    var entities = context.Model.GetEntityTypes().ToArray();
    Assert(entities.Length == 32, "Unexpected SchoolBot entity count.");
    Assert(entities.Select(entity => (entity.GetSchema(), entity.GetTableName())).Distinct().Count() == 32,
        "Unexpected SchoolBot table count.");
    foreach (var entity in entities)
    {
        var name = entity.ClrType.Name;
        var tableName = name == "SchoolBotCriterion" ? "SchoolBotCriteria" : name;
        Assert(entity.GetTableName() == tableName, $"Unexpected table mapping: {name}.");
        Assert(expectedProperties.TryGetValue(name, out var count) && entity.GetProperties().Count() == count,
            $"Unexpected property count: {name}.");
        var keyName = name == "SchoolBotCriterion" ? "SchoolBotCriteriaId" : name + "Id";
        var key = entity.FindPrimaryKey();
        Assert(key is not null && key.Properties.Count == 1 && key.Properties[0].Name == keyName
            && key.Properties[0].ClrType == typeof(int), $"Unexpected primary key: {name}.");
    }
    CheckProperty("SchoolBotSession", "TokenHash", typeof(string), false, 64);
    CheckProperty("SchoolBotSession", "SchoolId", typeof(int), false);
    CheckProperty("SchoolBotProject", "CurrentRevision", typeof(int), false);
    CheckProperty("SchoolBotIdempotency", "RequestKey", typeof(Guid), false);
    CheckProperty("SchoolBotIdempotency", "SchoolId", typeof(int?), true);
    CheckProperty("SchoolBotIdempotency", "RequestHash", typeof(string), false, 64);
    var propertyCount = entities.Sum(entity => entity.GetProperties().Count());
    Assert(propertyCount == 450, "Unexpected SchoolBot property count.");
    Console.WriteLine($"SchoolBot: 32 tables, {propertyCount} properties; primary keys and important fields verified.");

    void CheckProperty(string entityName, string propertyName, Type type, bool nullable, int? maxLength = null)
    {
        var property = entities.Single(entity => entity.ClrType.Name == entityName).FindProperty(propertyName);
        Assert(property is not null && property.ClrType == type && property.IsNullable == nullable
            && property.GetMaxLength() == maxLength, $"Unexpected field: {entityName}.{propertyName}.");
    }
}

static void CheckSchoolBotConfigurationError(string? connection, string error)
{
    Environment.SetEnvironmentVariable(DatabaseConnectionConfig.SchoolBotDbEnvVar, connection);
    var services = new ServiceCollection();
    services.AddSingleton<IDatabaseConnectionConfig>(new DatabaseConnectionConfig());
    services.AddDatabaseContexts();
    using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    using var scope = provider.CreateScope();
    try
    {
        scope.ServiceProvider.GetRequiredService<SchoolBotDbContext>();
    }
    catch (InvalidOperationException exception)
    {
        Assert(exception.Message == $"{error} database configuration: {DatabaseConnectionConfig.SchoolBotDbEnvVar}",
            "Unexpected SchoolBot configuration error or leaked connection details.");
        return;
    }
    throw new InvalidOperationException("Invalid or missing SchoolBot connection accepted.");
}

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
