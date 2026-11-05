using System.Text.Json;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed class LogsRepository(JabjaiSchoolLogsPostgreSQLDBContext db, TimeProvider clock) : ILogsRepository
{
    public async Task WriteLogAsync(string logType, string message, int? schoolId, int? userId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logType);
        ArgumentNullException.ThrowIfNull(message);
        db.ApiLogs.Add(new ApiLog
        {
            Applicationname = "SchoolBot", Path = logType, Schoolid = schoolId,
            Tstamp = clock.GetUtcNow().ToOffset(TimeSpan.FromHours(7)).DateTime,
            Requestbody = JsonSerializer.Serialize(new { message, userId }), Responsetime = "0"
        });
        await db.SaveChangesAsync(ct);
    }
}
