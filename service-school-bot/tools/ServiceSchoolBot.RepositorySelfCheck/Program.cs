using Microsoft.EntityFrameworkCore;
using Npgsql;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Repositories.Databases;

var connection = Environment.GetEnvironmentVariable("SCHOOL_BOT_REPOSITORY_TEST_CONNECTION")
    ?? throw new InvalidOperationException("Set SCHOOL_BOT_REPOSITORY_TEST_CONNECTION to the isolated test database.");
var target = new NpgsqlConnectionStringBuilder(connection);
if (target.Host is not ("localhost" or "127.0.0.1") || target.Database != "schoolbot_repo_tests")
    throw new InvalidOperationException("Repository checks require the isolated loopback schoolbot_repo_tests database.");
var previousKey = Environment.GetEnvironmentVariable("SCHOOL_BOT_REPLAY_KEY");
try
{
    Environment.SetEnvironmentVariable("SCHOOL_BOT_REPLAY_KEY", Convert.ToBase64String(new byte[32]));
    await RepositoryChecks.RunAsync(connection);
    await LegacyChecks.RunAsync(connection);
    Console.WriteLine("PASS: PostgreSQL persistence, atomic rollback, ETags, replay, tenant boundaries and all five database repositories.");
}
finally { Environment.SetEnvironmentVariable("SCHOOL_BOT_REPLAY_KEY", previousKey); }

internal static class Check
{
    public static void That(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static async Task RejectAsync(Func<Task> action, string? code = null)
    {
        try { await action(); }
        catch (ServiceSchoolBot.Domain.DomainRuleException error) when (code is null || error.Code == code) { return; }
        throw new InvalidOperationException($"Expected rejection: {code}");
    }

    public static SchoolBotDbContext Context(string connection) => new(
        new DbContextOptionsBuilder<SchoolBotDbContext>().UseNpgsql(connection).Options);

    public static SchoolBotUnitOfWork Repository(SchoolBotDbContext context) => new(context, TimeProvider.System);
}
