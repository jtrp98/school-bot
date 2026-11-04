using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;

namespace ServiceSchoolBot.Application.Services.Implementations.Curriculum;

public sealed partial class CurriculumService
{
    private Task<CurriculumMutationResponse> CommitMutationAsync(
        CurriculumPrincipalContext principal,
        string operation,
        string requestHash,
        Guid? key,
        int httpStatus,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<CurriculumMutationResponse>> mutate,
        CancellationToken ct) =>
        CommitMutationAsync(principal, operation, requestHash, key, httpStatus, mutate,
            body => (body.Id, body.UpdateDate), ct);

    private async Task<TResponse> CommitMutationAsync<TResponse>(
        CurriculumPrincipalContext principal,
        string operation,
        string requestHash,
        Guid? key,
        int httpStatus,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<TResponse>> mutate,
        Func<TResponse, (int Id, DateTimeOffset UpdateDate)> resource,
        CancellationToken ct)
    {
        if (dbAdapter is not IIdempotentSchoolBotCommit committer)
            throw new InvalidOperationException("The adapter does not support atomic idempotent commits.");
        var command = new IdempotencyCommand(null, $"staff_admin:{principal.PrincipalId}",
            operation, key ?? Guid.NewGuid(), requestHash);
        var committed = await committer.CommitWithIdempotencyAsync(command,
            async (transaction, resourceId, now, token) =>
            {
                var current = principal.RecheckAsync is null ? principal
                    : await principal.RecheckAsync(transaction, token);
                var access = operation switch
                {
                    "create_curriculum" => CurriculumAuthorizationService.EvaluateCreateCurriculum(current, now),
                    "publish_curriculum" => CurriculumAuthorizationService.EvaluatePublishCurriculum(current, null, now),
                    _ => CurriculumAuthorizationService.EvaluateUpdateCurriculumContent(current, now)
                };
                if (!access.IsAllowed) throw new DomainRuleException(access.ErrorCode!, access.Message!);
            },
            async (transaction, now, token) =>
            {
                var body = await mutate(transaction, now, token);
                var identity = resource(body);
                return new MutationResult<TResponse>(body, httpStatus,
                    identity.Id, OpaqueConcurrencyToken.ToETag(identity.UpdateDate));
            }, ct);
        return committed.Result.Body;
    }
}
