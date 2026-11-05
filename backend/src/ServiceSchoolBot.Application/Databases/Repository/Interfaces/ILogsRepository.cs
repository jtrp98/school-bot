using ServiceSchoolBot.Domain.Identity;

namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface ILogsRepository
{
    Task WriteLogAsync(string logType, string message, int? schoolId, int? userId, CancellationToken ct = default);
}
