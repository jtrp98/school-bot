using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using System.Collections.Concurrent;
using ServiceSchoolBot.Domain.Identity;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed class LogsRepository : ILogsRepository
{
    private readonly ConcurrentBag<(string LogType, string Message, int? SchoolId, int? UserId, DateTimeOffset Timestamp)> logs = new();

    public Task WriteLogAsync(string logType, string message, int? schoolId, int? userId, CancellationToken ct = default)
    {
        logs.Add((logType, message, schoolId, userId, DateTimeOffset.UtcNow));
        return Task.CompletedTask;
    }
}
