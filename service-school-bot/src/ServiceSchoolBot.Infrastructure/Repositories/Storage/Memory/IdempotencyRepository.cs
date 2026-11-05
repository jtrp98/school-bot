using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed class IdempotencyRepository(SchoolBotUnitOfWork unitOfWork) : IIdempotencyRepository
{
    private readonly SchoolBotRepositoryState state = unitOfWork.State;

    public async Task<IdempotentCommitResult<T>> CommitWithIdempotencyAsync<T>(
        IdempotencyCommand command,
        Func<ISchoolBotRepositoryScope, int?, DateTimeOffset, CancellationToken, Task> recheckAccess,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<MutationResult<T>>> mutate,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(recheckAccess);
        ArgumentNullException.ThrowIfNull(mutate);
        if (state.isCurriculumTransaction) throw new InvalidOperationException("Nested commits are not supported.");
        if (command.SchoolId is <= 0 || string.IsNullOrWhiteSpace(command.Principal) || command.Principal.Length > 512 ||
            string.IsNullOrWhiteSpace(command.Operation) || command.Operation.Length > 200 ||
            command.RequestKey == Guid.Empty || string.IsNullOrEmpty(command.RequestHash) ||
            command.RequestHash.Length != 64 ||
            command.RequestHash.Any(c => c is not (>= '0' and <= '9') and not (>= 'a' and <= 'f')))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Invalid idempotency command.");

        // shortcut: fake authorization snapshots precede the file lock; production requires the PostgreSQL transaction.
        var licenses = await state.backingStore.LoadLicenseUnitAsync(ct);
        var projects = await state.backingStore.LoadProjectsAsync(ct);
        var revisions = await state.backingStore.LoadProjectRevisionsAsync(ct);
        return await state.CurriculumStore.MutateCurriculumUnitAsync(async (snapshot, token) =>
        {
            var ledger = snapshot.Idempotency ?? [];
            var stored = ledger.SingleOrDefault(entry => entry.SchoolId == command.SchoolId &&
                string.Equals(entry.Principal, command.Principal, StringComparison.Ordinal) &&
                string.Equals(entry.Operation, command.Operation, StringComparison.Ordinal) &&
                entry.RequestKey == command.RequestKey);
            var transaction = await unitOfWork.CreateTransactionAsync(snapshot, token, licenses);
            await transaction.Store.CommitProjectAsync(projects, revisions, token);
            await recheckAccess(transaction.Adapter, stored?.ResourceId, state.clock.GetUtcNow(), token);
            if (stored is not null)
            {
                if (!string.Equals(stored.RequestHash, command.RequestHash, StringComparison.Ordinal))
                    throw new DomainRuleException(ErrorCodes.IdempotencyConflict, "Payload mismatch for idempotency key.");
                if (state.clock.GetUtcNow() >= stored.ReplayUntil)
                    throw new DomainRuleException(ErrorCodes.IdempotencyExpired, "Idempotency replay window has expired.");
                var replay = CurriculumReplayCipher.Decrypt<T>(stored, command);
                return new CurriculumStoreMutation<IdempotentCommitResult<T>>(snapshot, new(replay, true), false);
            }

            var result = await mutate(transaction.Adapter, state.clock.GetUtcNow(), token);
            if (result.HttpStatus is < 200 or >= 300)
                throw new InvalidOperationException("Only successful mutations may be committed.");
            var encrypted = CurriculumReplayCipher.Encrypt(result, command);
            var completed = state.clock.GetUtcNow();
            var entry = new CurriculumIdempotencyRecord(
                ledger.Count == 0 ? 1 : checked(ledger.Max(e => e.SchoolBotIdempotencyId) + 1),
                command.Principal, command.Operation, command.RequestKey, command.RequestHash, null,
                result.HttpStatus, encrypted.Cipher, encrypted.KeyId, result.ResourceId,
                completed, completed.AddHours(24), false, null, completed, null, completed, command.SchoolId);
            var candidate = await transaction.Store.LoadCurriculumUnitAsync(token);
            candidate = candidate with { Idempotency = [.. ledger, entry] };
            token.ThrowIfCancellationRequested();
            return new CurriculumStoreMutation<IdempotentCommitResult<T>>(candidate, new(result, false));
        }, ct);
    }

}
