using System.Security.Cryptography;
using System.Text;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;

namespace ServiceSchoolBot.Application.Services.Implementations.Assessment;

public sealed class AttemptFinalizer(IAssessmentRepository repository)
{
    public async Task<(int Completed, int Deferred)> SweepAsync(int limit = 50, CancellationToken ct = default)
    {
        if (repository is not IIdempotentSchoolBotCommit commit)
            throw new DomainRuleException(ErrorCodes.ServiceUnavailable, "Atomic expiry storage is required.");
        var completed = 0;
        var deferred = 0;
        foreach (var candidate in await repository.ListExpirableAttemptsAsync(limit, ct))
        {
            var fingerprint = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes($"expiry:{candidate.SchoolBotAttemptId}")));
            try
            {
                var result = await commit.CommitWithIdempotencyAsync(new(candidate.SchoolId, "service:attempt-finalizer",
                    "expire_attempt", new Guid(Convert.FromHexString(fingerprint).AsSpan(0, 16)), fingerprint),
                    (_, _, _, _) => Task.CompletedTask, async (scope, _, token) =>
                    {
                        var current = await scope.GetAttemptByIdAsync(candidate.SchoolBotAttemptId, token)
                            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Attempt not found.");
                        var now = await scope.GetAssessmentTimeAsync(token);
                        if (current.SchoolId != candidate.SchoolId) throw new DomainRuleException(ErrorCodes.SchoolMismatch, "Attempt scope changed.");
                        var saved = AttemptEvaluation.Apply(current, null, false, now, null);
                        if (saved != current)
                            saved = await scope.WriteAttemptAsync(saved,
                                new(0, "service:attempt-finalizer", "Attempt.Expired", "SchoolBotAttempt", current.SchoolBotAttemptId,
                                    Guid.NewGuid(), "Assessment deadline reached", false, null, now, null, now, current.SchoolId), token);
                        return new MutationResult<SchoolBotAttemptRecord>(saved, 200, saved.SchoolBotAttemptId, null);
                    }, ct);
                if (result.Result.Body.State == "expired") completed++;
            }
            catch (DomainRuleException ex) when (ex.Code == ErrorCodes.GraderUnavailable) { deferred++; }
        }
        return (completed, deferred);
    }

    public static async Task FinalizeInTransactionAsync(ISchoolBotRepositoryScope scope, SchoolBotAttemptRecord attempt, CancellationToken ct)
    {
        var now = await scope.GetAssessmentTimeAsync(ct);
        if (!(attempt.State == "started" && attempt.DeadlineAt <= now || attempt.State == "expired" && attempt.EarnedScore is null)) return;
        var saved = AttemptEvaluation.Apply(attempt, null, false, now, null);
        await scope.WriteAttemptAsync(saved, new(0, "service:attempt-finalizer", "Attempt.Expired", "SchoolBotAttempt",
            attempt.SchoolBotAttemptId, Guid.NewGuid(), "Assessment deadline reached before score calculation", false,
            null, now, null, now, attempt.SchoolId), ct);
    }
}
