using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using System.Security.Cryptography;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed partial class IdempotencyRepository(SchoolBotDbContext db, SchoolBotUnitOfWork unitOfWork) : IIdempotencyRepository
{
    public Task<IdempotentCommitResult<T>> CommitWithIdempotencyAsync<T>(
        IdempotencyCommand command,
        Func<ISchoolBotRepositoryScope, int?, DateTimeOffset, CancellationToken, Task> recheckAccess,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<MutationResult<T>>> mutate,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(recheckAccess);
        ArgumentNullException.ThrowIfNull(mutate);
        if (unitOfWork.IdempotentCommit || db.Database.CurrentTransaction is not null)
            throw new InvalidOperationException("Nested commits are not supported.");
        if (command.SchoolId is <= 0 || string.IsNullOrWhiteSpace(command.Principal) || command.Principal.Length > 512 ||
            string.IsNullOrWhiteSpace(command.Operation) || command.Operation.Length > 200 || command.RequestKey == Guid.Empty ||
            command.RequestHash is not { Length: 64 } || command.RequestHash.Any(c => c is not (>= '0' and <= '9') and not (>= 'a' and <= 'f')))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Invalid idempotency command.");
        ValidateReplayKey();
        return unitOfWork.WriteAsync(async () =>
        {
            unitOfWork.IdempotentCommit = true;
            try
            {
                var stored = await db.SchoolBotIdempotencies.AsNoTracking().SingleOrDefaultAsync(row =>
                    row.SchoolId == command.SchoolId && row.Principal == command.Principal &&
                    row.Operation == command.Operation && row.RequestKey == command.RequestKey, ct);
                await recheckAccess(unitOfWork, stored?.ResourceId, unitOfWork.Now(), ct);
                if (stored is not null)
                {
                    if (stored.RequestHash != command.RequestHash)
                        throw new DomainRuleException(ErrorCodes.IdempotencyConflict, "Payload mismatch for idempotency key.");
                    var entry = From(stored);
                    if (unitOfWork.Now() >= entry.ReplayUntil)
                        throw new DomainRuleException(ErrorCodes.IdempotencyExpired, "Idempotency replay window has expired.");
                    return new IdempotentCommitResult<T>(CurriculumReplayCipher.Decrypt<T>(entry, command), true);
                }
                var result = await mutate(unitOfWork, unitOfWork.Now(), ct);
                if (result.HttpStatus is < 200 or >= 300)
                    throw new InvalidOperationException("Only successful mutations may be committed.");
                var encrypted = CurriculumReplayCipher.Encrypt(result, command);
                var now = unitOfWork.Now();
                db.SchoolBotIdempotencies.Add(To(new CurriculumIdempotencyRecord(
                    0, command.Principal, command.Operation, command.RequestKey, command.RequestHash, null,
                    result.HttpStatus, encrypted.Cipher, encrypted.KeyId, result.ResourceId, now, now.AddHours(24),
                    false, null, now, null, now, command.SchoolId)));
                return new IdempotentCommitResult<T>(result, false);
            }
            finally { unitOfWork.IdempotentCommit = false; }
        }, ct);
    }

    private static void ValidateReplayKey()
    {
        try
        {
            var configured = Environment.GetEnvironmentVariable("SCHOOL_BOT_REPLAY_KEY");
            if (configured is null) throw new FormatException();
            var key = Convert.FromBase64String(configured);
            try { if (key.Length != 32) throw new FormatException(); }
            finally { CryptographicOperations.ZeroMemory(key); }
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("PostgreSQL mutations require a configured 32-byte SCHOOL_BOT_REPLAY_KEY.");
        }
    }
}
