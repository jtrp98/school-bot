using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Identity;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed partial class WebSessionRepository(SchoolBotDbContext db, SchoolBotUnitOfWork unitOfWork) : IWebSessionRepository
{
    public async Task<SchoolBotSessionRecord?> GetSessionByTokenHashAsync(
        string tokenHash, CancellationToken ct = default)
    {
        var row = await db.SchoolBotSessions.AsNoTracking()
            .Where(r => r.TokenHash.ToLower() == tokenHash.ToLower() && !r.IsDelete)
            .SingleOrDefaultAsync(ct);
        return row is null ? null : From(row);
    }

    public Task<SchoolBotSessionRecord> CreateSessionAsync(SchoolBotSessionRecord session, CancellationToken ct = default) =>
        unitOfWork.WriteAsync(async () =>
        {
            if (session.SchoolId <= 0 || string.IsNullOrWhiteSpace(session.TokenHash))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "Invalid session.");
            if (await db.SchoolBotSessions.AnyAsync(row => row.TokenHash == session.TokenHash, ct))
                throw new DomainRuleException(ErrorCodes.StateConflict, "Session token already exists.");
            var now = unitOfWork.Now();
            var row = To(session with
            {
                SchoolBotSessionId = 0, CreateDate = session.CreateDate ?? now, UpdateDate = now, IsDelete = false
            });
            db.SchoolBotSessions.Add(row);
            await db.SaveChangesAsync(ct);
            return From(row);
        }, ct);

    public Task<SchoolBotSessionRecord> UpdateSessionAsync(
        SchoolBotSessionRecord session, string? expectedETag = null, CancellationToken ct = default) =>
        unitOfWork.WriteAsync(async () =>
        {
            var row = await db.SchoolBotSessions.SingleOrDefaultAsync(r =>
                r.SchoolBotSessionId == session.SchoolBotSessionId && r.SchoolId == session.SchoolId && !r.IsDelete, ct)
                ?? throw SchoolBotUnitOfWork.Missing("Session");
            SchoolBotUnitOfWork.Match(expectedETag, row.UpdateDate);
            if (row.TokenHash != session.TokenHash || row.AccountId != session.AccountId)
                throw new DomainRuleException(ErrorCodes.StateConflict, "Session identity cannot change.");
            var updated = session with { UpdateDate = unitOfWork.Now(From(row).UpdateDate) };
            db.Entry(row).CurrentValues.SetValues(To(updated));
            await db.SaveChangesAsync(ct);
            return From(row);
        }, ct);

    public async Task RevokeSessionAsync(string tokenHash, int? revokedBy, CancellationToken ct = default) =>
        await unitOfWork.WriteAsync(async () =>
        {
            var rows = await db.SchoolBotSessions.Where(r => r.TokenHash.ToLower() == tokenHash.ToLower() && !r.IsDelete)
            .ToArrayAsync(ct);
            foreach (var row in rows)
            {
                var now = unitOfWork.Now(From(row).UpdateDate).UtcDateTime;
                row.RevokedAt = now;
                row.UpdateBy = revokedBy;
                row.UpdateDate = now;
            }
            return true;
        }, ct);
}
