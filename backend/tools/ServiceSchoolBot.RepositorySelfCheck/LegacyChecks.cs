using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using Master = ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;
using School = ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;
using Logs = ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;
using Grade = ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

internal static class LegacyChecks
{
    public static async Task RunAsync(string connection)
    {
        await using var master = new JabjaiMasterSingleDBPostgreSQLDBContext(
            new DbContextOptionsBuilder<JabjaiMasterSingleDBPostgreSQLDBContext>().UseNpgsql(connection).Options);
        await using var school = new JabjaiSchoolSingleDBPostgreSQLDBContext(
            new DbContextOptionsBuilder<JabjaiSchoolSingleDBPostgreSQLDBContext>().UseNpgsql(connection).Options);
        await using var logs = new JabjaiSchoolLogsPostgreSQLDBContext(
            new DbContextOptionsBuilder<JabjaiSchoolLogsPostgreSQLDBContext>().UseNpgsql(connection).Options);
        await using var grade = new PGGradeDBPostgreSQLDBContext(
            new DbContextOptionsBuilder<PGGradeDBPostgreSQLDBContext>().UseNpgsql(connection).Options);
        await SetupAsync<Master.TmasterDbuser>(master, ["SId", "NCompany", "Username", "CType", "CDel"]);
        await SetupAsync<Master.Tcompany>(master, ["NCompany", "IsActive", "CDel"]);
        await SetupAsync<School.Tuser>(school, ["SId", "SchoolId", "SName", "SLastname", "CDel"]);
        await SetupAsync<School.Temployee>(school, ["SEmp", "SchoolId", "SName", "SLastname", "CDel"]);
        await SetupAsync<Logs.ApiLog>(logs, null);
        await SetupAsync<Grade.Pgtgrade>(grade, ["NGradeId", "SchoolId", "CDel"]);
        await SetupAsync<Grade.PgtgradeDetail>(grade, ["SchoolId"]);
        await SetupAsync<Grade.PgtgradeAuditLog>(grade, ["SchoolId"]);

        var id = Random.Shared.Next(10000, 100000000);
        await InsertAsync<Master.TmasterDbuser>(master, new() { ["SId"] = id, ["NCompany"] = id,
            ["Username"] = "test-" + id, ["CType"] = "0", ["CDel"] = '0' });
        await InsertAsync<Master.Tcompany>(master, new() { ["NCompany"] = id, ["IsActive"] = true, ["CDel"] = false });
        await InsertAsync<School.Tuser>(school, new() { ["SId"] = id, ["SchoolId"] = id,
            ["SName"] = "Student", ["SLastname"] = "Local", ["CDel"] = '0' });
        await InsertAsync<School.Temployee>(school, new() { ["SEmp"] = id, ["SchoolId"] = id,
            ["SName"] = "Employee", ["SLastname"] = "Local", ["CDel"] = '0' });
        await InsertAsync<Grade.Pgtgrade>(grade, new() { ["NGradeId"] = id, ["SchoolId"] = id, ["CDel"] = false });
        var accounts = new MasterAccountRepository(master);
        Check.That((await accounts.FindAccountAsync(id, "test-" + id))?.AccountId == id, "Master account query failed.");
        Check.That(await accounts.FindAccountByIdAsync(id + 1, id) is null, "Master account crossed schools.");
        Check.That(await accounts.IsSchoolActiveAsync(id) && !await accounts.IsSchoolActiveAsync(id + 1),
            "Unknown school was treated as active.");
        var profiles = new SchoolProfileRepository(school);
        Check.That(await profiles.IsStudentActiveAsync(id, id), "School student query failed.");
        Check.That(!await profiles.IsStudentActiveAsync(id + 1, id), "Student query crossed schools.");
        Check.That((await profiles.FindEmployeeProfileAsync(id, id)) is { IsActive: true, IsAdmin: false },
            "Employee query or admin boundary failed.");
        Check.That(!await profiles.IsEmployeeActiveAsync(id + 1, id), "Employee query crossed schools.");
        var logRepository = new LogsRepository(logs, TimeProvider.System);
        await logRepository.WriteLogAsync("repository-self-check", "synthetic message", id, id);
        Check.That(await logs.ApiLogs.AsNoTracking().AnyAsync(r => r.Schoolid == id && r.Applicationname == "SchoolBot"),
            "Logs repository did not persist to the logs context.");
        var gradeRepository = new GradeRepository(grade);
        Check.That(await gradeRepository.VerifyGradeWriterReadyAsync(id), "Grade readiness did not query the grade database.");
        Check.That(!await gradeRepository.VerifyGradeWriterReadyAsync(id + 1), "Grade readiness ignored school context.");
        Console.WriteLine("PASS: Master/School tenant and active filters, Logs persistence and real Grade readiness.");
    }

    private static async Task SetupAsync<T>(DbContext context, string[]? properties) where T : class
    {
        var entity = context.Model.FindEntityType(typeof(T))!;
        var schema = entity.GetSchema() ?? "public";
        var table = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
        var selected = entity.GetProperties().Where(p => properties is null || properties.Contains(p.Name)).ToArray();
        var columns = selected.Select(property =>
        {
            var identity = properties is null && property.IsPrimaryKey() && property.ClrType == typeof(int)
                ? " GENERATED BY DEFAULT AS IDENTITY" : "";
            return $"{Quote(property.GetColumnName(table)!)} {property.GetColumnType()}{identity}";
        });
        // This fixture runs only after Program verifies an isolated loopback test database.
        var createSchema = $"CREATE SCHEMA IF NOT EXISTS {Quote(schema)}";
        var createTable = $"CREATE TABLE IF NOT EXISTS {Quote(schema)}.{Quote(table.Name)} ({string.Join(", ", columns)})";
        await context.Database.ExecuteSqlRawAsync(createSchema);
        await context.Database.ExecuteSqlRawAsync(createTable);
    }

    private static async Task InsertAsync<T>(DbContext context, Dictionary<string, object> values) where T : class
    {
        var entity = context.Model.FindEntityType(typeof(T))!;
        var table = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
        var columns = values.Keys.Select(name => Quote(entity.FindProperty(name)!.GetColumnName(table)!));
        var parameters = values.Values.Select((_, index) => "{" + index + "}").ToArray();
        var sql = $"INSERT INTO {Quote(entity.GetSchema() ?? "public")}.{Quote(table.Name)} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)})";
        await context.Database.ExecuteSqlRawAsync(sql, values.Values.ToArray());
    }

    private static string Quote(string identifier) => "\"" + identifier.Replace("\"", "\"\"") + "\"";
}
