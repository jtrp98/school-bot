namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public sealed record IdempotencyCommand(
    int? SchoolId,
    string Principal,
    string Operation,
    Guid RequestKey,
    string RequestHash);

public sealed record MutationResult<T>(
    T Body,
    int HttpStatus,
    int? ResourceId,
    string? ETag);

public sealed record IdempotentCommitResult<T>(MutationResult<T> Result, bool Replayed);

public interface IIdempotentSchoolBotCommit
{
    bool IsInAtomicCommit => false;
    Task<IdempotentCommitResult<T>> CommitWithIdempotencyAsync<T>(
        IdempotencyCommand command,
        Func<ISchoolBotRepositoryScope, int?, DateTimeOffset, CancellationToken, Task> recheckAccess,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<MutationResult<T>>> mutate,
        CancellationToken ct = default);
}
