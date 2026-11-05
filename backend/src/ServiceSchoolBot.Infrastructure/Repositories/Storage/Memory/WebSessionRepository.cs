using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Identity;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed class WebSessionRepository : IWebSessionRepository
{
    private readonly SchoolBotRepositoryState state;

    public WebSessionRepository(SchoolBotUnitOfWork unitOfWork)
    {
        state = unitOfWork.State;
    }

    public Task<SchoolBotSessionRecord> CreateSessionAsync(SchoolBotSessionRecord session, CancellationToken ct = default) =>
        state.CurriculumStore.MutateCurriculumUnitAsync((snapshot, token) =>
        {

            var rows = snapshot.Sessions ?? [];
            var now = state.clock.GetUtcNow();
            var created = session with
            {
                SchoolBotSessionId = rows.Count == 0 ? 1 : checked(rows.Max(row => row.SchoolBotSessionId) + 1),
                CreateDate = session.CreateDate ?? now,
                UpdateDate = now,
                IsDelete = false
            };
            return Task.FromResult(new CurriculumStoreMutation<SchoolBotSessionRecord>(
                snapshot with { Sessions = [.. rows, created] }, created));
        }, ct);

    public async Task<SchoolBotSessionRecord?> GetSessionByTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        (await state.backingStore.LoadSessionsAsync(ct)).FirstOrDefault(session => !session.IsDelete &&
            string.Equals(session.TokenHash, tokenHash, StringComparison.OrdinalIgnoreCase));

    public Task<SchoolBotSessionRecord> UpdateSessionAsync(
        SchoolBotSessionRecord session, string? expectedETag = null, CancellationToken ct = default) =>
        state.CurriculumStore.MutateCurriculumUnitAsync((snapshot, token) =>
        {
            var rows = (snapshot.Sessions ?? []).ToArray();
            var index = Array.FindIndex(rows, row => row.SchoolBotSessionId == session.SchoolBotSessionId && !row.IsDelete);
            if (index < 0) throw new DomainRuleException(ErrorCodes.NotFound, "Session not found.");
            if (expectedETag is not null && !OpaqueConcurrencyToken.Matches(expectedETag, rows[index].UpdateDate))
                throw new DomainRuleException(ErrorCodes.VersionConflict, "Concurrency token mismatch.");
            var updated = session with { UpdateDate = state.clock.GetUtcNow() };
            rows[index] = updated;
            return Task.FromResult(new CurriculumStoreMutation<SchoolBotSessionRecord>(
                snapshot with { Sessions = rows }, updated));
        }, ct);

    public Task RevokeSessionAsync(string tokenHash, int? revokedBy, CancellationToken ct = default) =>
        state.CurriculumStore.MutateCurriculumUnitAsync((snapshot, token) =>
        {
            var now = state.clock.GetUtcNow();
            var rows = (snapshot.Sessions ?? []).Select(row =>
                !row.IsDelete && string.Equals(row.TokenHash, tokenHash, StringComparison.OrdinalIgnoreCase)
                    ? row with { RevokedAt = now, UpdateBy = revokedBy, UpdateDate = now }
                    : row).ToArray();
            return Task.FromResult(new CurriculumStoreMutation<bool>(snapshot with { Sessions = rows }, true));
        }, ct);
}
