using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.CurriculumCommitSelfCheckFixture;

namespace ServiceSchoolBot.Tests;

public sealed class CurriculumAtomicFailureSelfChecks
{
    public static IEnumerable<object[]> Operations => CurriculumIdempotencySelfChecks.Operations;

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task FailureBeforeCommitRollsBackAndLostResponseReplays(bool file, string operation)
    {
        var fixture = new CurriculumCommitSelfCheckFixture(file);
        var mutation = await fixture.Prepare(operation);
        var baseline = Json(await fixture.Snapshot());
        var key = Guid.NewGuid();
        var (service, gate) = fixture.Gated();
        gate.FailAfterMutation = true;
        await Assert.ThrowsAsync<IOException>(() => mutation.Run(service, key));
        Assert.Equal(baseline, Json(await fixture.Snapshot()));
        gate.FailAfterMutation = false;
        gate.LoseResponse = true;
        await Assert.ThrowsAsync<IOException>(() => mutation.Run(service, key));
        var afterCommit = await fixture.Snapshot();
        var recovered = await mutation.Run(fixture.Service(), key);
        Assert.Equal(afterCommit.Idempotency!.Last().ResourceId, recovered.Id);
        Assert.Equal(Json(afterCommit), Json(await fixture.Snapshot()));
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task CancellingWaiterLeavesFirstMutationIntact(bool file, string operation)
    {
        var fixture = new CurriculumCommitSelfCheckFixture(file);
        var mutation = await fixture.Prepare(operation);
        var before = await fixture.Snapshot();
        var (firstService, firstGate) = fixture.Gated();
        var (secondService, secondGate) = fixture.Gated();
        firstGate.PauseInside = true;
        var key = Guid.NewGuid();
        var first = mutation.Run(firstService, key);
        try
        {
            await firstGate.Inside.Task.WaitAsync(TimeSpan.FromSeconds(10));
            using var cancellation = new CancellationTokenSource();
            var second = mutation.Run(secondService, key, ct: cancellation.Token);
            await secondGate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.False(second.IsCompleted);
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await second);
            firstGate.Release.TrySetResult();
            var success = await first;
            Assert.Equal(success, await mutation.Run(fixture.Service(), key));
            var after = await fixture.Snapshot();
            Assert.Equal(before.AuditEvents.Count + 1, after.AuditEvents.Count);
            Assert.Equal((before.Idempotency?.Count ?? 0) + 1, after.Idempotency!.Count);
        }
        finally { firstGate.Release.TrySetResult(); }
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task WaitingReplayRechecksRolesAndClockInsideStoreLock(bool file, string operation)
    {
        var fixture = new CurriculumCommitSelfCheckFixture(file);
        var mutation = await fixture.Prepare(operation);
        var roleUntil = fixture.Clock.Now.AddHours(1);
        var actor = mutation.Actor with
        {
            Roles = mutation.Actor.Roles!.Select(role => role with { ValidUntil = roleUntil }).ToArray()
        };
        var key = Guid.NewGuid();
        await mutation.Run(fixture.Service(), key, actor);
        var baseline = Json(await fixture.Snapshot());
        var (service, gate) = fixture.Gated();
        var blocker = ((ICurriculumBackingStore)fixture.Store()).MutateCurriculumUnitAsync(async (snapshot, token) =>
        {
            gate.Inside.TrySetResult();
            await gate.Release.Task.WaitAsync(TimeSpan.FromSeconds(10), token);
            return new CurriculumStoreMutation<bool>(snapshot, true, false);
        });
        try
        {
            await gate.Inside.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var retry = mutation.Run(service, key, actor);
            await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.False(retry.IsCompleted);
            fixture.Clock.Now = roleUntil;
            gate.Release.TrySetResult();
            await blocker;
            Assert.Equal(ErrorCodes.StaffRoleRequired,
                (await Assert.ThrowsAsync<DomainRuleException>(async () => await retry)).Code);
            Assert.Equal(baseline, Json(await fixture.Snapshot()));
        }
        finally { gate.Release.TrySetResult(); }
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task ReplayExpiryIsExclusiveAndHashConflictTakesPrecedence(bool file, string operation)
    {
        var fixture = new CurriculumCommitSelfCheckFixture(file);
        var mutation = await fixture.Prepare(operation);
        var key = Guid.NewGuid();
        var success = await mutation.Run(fixture.Service(), key);
        var committed = await fixture.Snapshot();
        var until = committed.Idempotency!.Last().ReplayUntil;
        fixture.Clock.Now = until.AddTicks(-1);
        Assert.Equal(success, await mutation.Run(fixture.Service(), key));
        foreach (var now in new[] { until, until.AddTicks(1) })
        {
            fixture.Clock.Now = now;
            Assert.Equal(ErrorCodes.IdempotencyExpired, (await Assert.ThrowsAsync<DomainRuleException>(() =>
                mutation.Run(fixture.Service(), key))).Code);
            Assert.Equal(ErrorCodes.IdempotencyConflict, (await Assert.ThrowsAsync<DomainRuleException>(() =>
                mutation.Run(fixture.Service(), key, different: true))).Code);
            Assert.Equal(Json(committed), Json(await fixture.Snapshot()));
        }
    }
}
