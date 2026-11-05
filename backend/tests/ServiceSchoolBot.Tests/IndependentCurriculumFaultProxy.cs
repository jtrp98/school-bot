using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using System.Reflection;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

public class IndependentCurriculumFaultProxy : DispatchProxy, IIdempotentSchoolBotCommit
{
    public SchoolBotUnitOfWork Target { get; set; } = null!;
    public bool FailEncoding { get; set; }
    public bool LoseResponse { get; set; }
    public bool MutationFinished { get; private set; }

    protected override object? Invoke(MethodInfo? method, object?[]? arguments) => method!.Invoke(Target, arguments);

    public async Task<IdempotentCommitResult<T>> CommitWithIdempotencyAsync<T>(IdempotencyCommand command,
        Func<ISchoolBotRepositoryScope, int?, DateTimeOffset, CancellationToken, Task> recheckAccess,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<MutationResult<T>>> mutate,
        CancellationToken ct = default)
    {
        if (FailEncoding)
        {
            await Target.CommitWithIdempotencyAsync(command, recheckAccess, async (facade, now, token) =>
            {
                var result = await mutate(facade, now, token);
                MutationFinished = true;
                return new MutationResult<UnencodableResult>(new(), result.HttpStatus, result.ResourceId, result.ETag);
            }, ct);
            throw new InvalidOperationException("Encoding fault was not reached.");
        }
        var committed = await Target.CommitWithIdempotencyAsync(command, recheckAccess, mutate, ct);
        if (LoseResponse) throw new IOException("Independent response loss after durable commit.");
        return committed;
    }

    public sealed class UnencodableResult
    {
        public string Body => throw new IOException("Independent result encoding failure.");
    }
}

public class IndependentCurriculumStoreFaultProxy : DispatchProxy, ICurriculumBackingStore
{
    public ISchoolBotBackingStore Target { get; set; } = null!;
    public bool FailBeforeSwap { get; set; }
    public bool CandidateComplete { get; private set; }

    protected override object? Invoke(MethodInfo? method, object?[]? arguments) => method!.Invoke(Target, arguments);

    public Task<CurriculumStoreSnapshot> LoadCurriculumUnitAsync(CancellationToken ct = default) =>
        ((ICurriculumBackingStore)Target).LoadCurriculumUnitAsync(ct);

    public Task<T> MutateCurriculumUnitAsync<T>(
        Func<CurriculumStoreSnapshot, CancellationToken, Task<CurriculumStoreMutation<T>>> mutate,
        CancellationToken ct = default) =>
        ((ICurriculumBackingStore)Target).MutateCurriculumUnitAsync(async (snapshot, token) =>
        {
            var candidate = await mutate(snapshot, token);
            CandidateComplete = candidate.Snapshot.Idempotency?.Count > (snapshot.Idempotency?.Count ?? 0);
            if (FailBeforeSwap) throw new IOException("Independent failure before backing store swap/replace.");
            return candidate;
        }, ct);
}