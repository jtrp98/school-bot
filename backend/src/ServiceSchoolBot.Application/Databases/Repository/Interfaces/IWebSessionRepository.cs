using ServiceSchoolBot.Domain.Identity;

namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface IWebSessionRepository
{
    Task<SchoolBotSessionRecord> CreateSessionAsync(SchoolBotSessionRecord session, CancellationToken ct = default);
    Task<SchoolBotSessionRecord?> GetSessionByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task<SchoolBotSessionRecord> UpdateSessionAsync(SchoolBotSessionRecord session, string? expectedETag = null, CancellationToken ct = default);
    Task RevokeSessionAsync(string tokenHash, int? revokedBy, CancellationToken ct = default);
}
