using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Domain;
using static ServiceSchoolBot.Tests.CurriculumCommitSelfCheckFixture;

namespace ServiceSchoolBot.Tests;

public sealed class CurriculumIdempotencySelfChecks
{
    public static IEnumerable<object[]> Operations => from file in new[] { false, true }
        from operation in new[] { "create", "metadata", "content", "publish" }
        select new object[] { file, operation };
    public static IEnumerable<object[]> Replays => from item in Operations
        from concurrent in new[] { false, true } from different in new[] { false, true }
        select item.Concat(new object[] { concurrent, different }).ToArray();

    [Theory]
    [MemberData(nameof(Replays))]
    public async Task SameTupleReplaysOrConflictsUnderStoreLock(bool file, string operation, bool concurrent, bool different)
    {
        var fixture = new CurriculumCommitSelfCheckFixture(file);
        var mutation = await fixture.Prepare(operation);
        var baseline = await fixture.Snapshot();
        var (firstService, firstGate) = fixture.Gated();
        var (secondService, secondGate) = fixture.Gated();
        firstGate.PauseInside = concurrent;
        var key = Guid.NewGuid();
        var first = mutation.Run(firstService, key);
        try
        {
            if (concurrent) await firstGate.Inside.Task.WaitAsync(TimeSpan.FromSeconds(10));
            else await first;
            var second = mutation.Run(secondService, key, different: different);
            await secondGate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            if (concurrent) Assert.False(second.IsCompleted);
            firstGate.Release.TrySetResult();
            var original = await first;
            if (different)
                Assert.Equal(ErrorCodes.IdempotencyConflict,
                    (await Assert.ThrowsAsync<DomainRuleException>(async () => await second)).Code);
            else Assert.Equal(original, await second);
            var after = await fixture.Snapshot();
            Assert.Equal(baseline.Curricula.Count + (operation == "create" ? 1 : 0), after.Curricula.Count);
            Assert.Equal(baseline.AuditEvents.Count + 1, after.AuditEvents.Count);
            Assert.Equal((baseline.Idempotency?.Count ?? 0) + 1, after.Idempotency!.Count);
            var entry = after.Idempotency.Last();
            Assert.Equal(operation == "create" ? 201 : 200, entry.HttpStatus);
            Assert.Equal(original.Id, entry.ResourceId);
            Assert.Null(entry.SchoolId);
            Assert.Equal($"staff_admin:{mutation.Actor.PrincipalId}", entry.Principal);
            Assert.NotEmpty(entry.ResultCipher!);
            Assert.NotNull(entry.ResultKeyId);
            Assert.DoesNotContain(nameof(CurriculumMutationResponse), System.Text.Encoding.UTF8.GetString(entry.ResultCipher!));
            var retrySnapshot = Json(after);
            Assert.Equal(original, await mutation.Run(fixture.Service(), key));
            Assert.Equal(retrySnapshot, Json(await fixture.Snapshot()));
            if (operation == "content")
            {
                Assert.Equal(2, after.Lessons.Count);
                Assert.Equal(6, after.Missions.Count);
                Assert.Equal(6, after.Criteria.Count);
                Assert.Equal(6, after.Activities.Count);
            }
        }
        finally { firstGate.Release.TrySetResult(); }
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task ActorsHaveSeparateTuplesAndDomainChecks(bool file, string operation)
    {
        var fixture = new CurriculumCommitSelfCheckFixture(file);
        var firstCase = await fixture.Prepare(operation);
        var secondCase = await fixture.Prepare(operation);
        var baseline = await fixture.Snapshot();
        var firstService = fixture.Service();
        var secondService = fixture.Service();
        var key = Guid.NewGuid();
        var first = await firstCase.Run(firstService, key);
        var second = await secondCase.Run(secondService, key, operation == "publish" ? SecondApprover : Editor);
        Assert.NotEqual(first.Id, second.Id);
        var after = await fixture.Snapshot();
        Assert.Equal(baseline.AuditEvents.Count + 2, after.AuditEvents.Count);
        Assert.Equal((baseline.Idempotency?.Count ?? 0) + 2, after.Idempotency!.Count);
        Assert.Equal(2, after.Idempotency.TakeLast(2).Select(e => e.Principal).Distinct().Count());
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task ActorDomainCollisionsUseFreshScope(bool file, string operation)
    {
        var fixture = new CurriculumCommitSelfCheckFixture(file);
        var firstCase = await fixture.Prepare(operation);
        var key = Guid.NewGuid();
        var first = await firstCase.Run(fixture.Service(), key);
        var secondService = fixture.Service();
        var after = await fixture.Snapshot();
        if (operation == "create")
        {
            Assert.Equal(ErrorCodes.StateConflict, (await Assert.ThrowsAsync<DomainRuleException>(() =>
                firstCase.Run(secondService, key, Editor))).Code);
        }
        else if (operation is "metadata" or "content")
        {
            Assert.Equal(ErrorCodes.VersionConflict, (await Assert.ThrowsAsync<DomainRuleException>(() =>
                firstCase.Run(secondService, key, Editor))).Code);
            fixture.Clock.Now = fixture.Clock.Now.AddTicks(1);
            await firstCase.Run(secondService, key, Editor, current: first);
            var fresh = await fixture.Snapshot();
            Assert.Equal(after.AuditEvents.Count + 1, fresh.AuditEvents.Count);
            Assert.Equal(Editor.PrincipalId, fresh.AuditEvents.Last().Actor);
        }
        else
        {
            Assert.Equal(ErrorCodes.StateConflict, (await Assert.ThrowsAsync<DomainRuleException>(() =>
                firstCase.Run(secondService, key, SecondApprover, different: true))).Code);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OneActorCanUseSameKeyAcrossFourOperations(bool file)
    {
        var fixture = new CurriculumCommitSelfCheckFixture(file);
        var staff = Author with { Roles = [.. Author.Roles!, .. Approver.Roles!] };
        var otherDraft = await fixture.Prepare("publish", Editor);
        var baseline = await fixture.Snapshot();
        var key = Guid.NewGuid();
        var service = fixture.Service();
        var input = Metadata();
        var created = await service.CreateCurriculumAsync(staff, input, idempotencyKey: key);
        fixture.Clock.Now = fixture.Clock.Now.AddTicks(1);
        var metadata = new CurriculumMetadataInput(1, "updated", "source:update", ["P1"], []);
        var edited = await service.UpdateMetadataAsync(staff, created.Id, metadata, Tag(created), idempotencyKey: key);
        fixture.Clock.Now = fixture.Clock.Now.AddTicks(1);
        var content = Content();
        var contentResult = await service.UpdateContentAsync(staff, created.Id, content, Tag(edited), idempotencyKey: key);
        var publishResult = await otherDraft.Run(fixture.Service(), key, staff);
        Assert.Equal(created, await service.CreateCurriculumAsync(staff, input, idempotencyKey: key));
        Assert.Equal(edited, await service.UpdateMetadataAsync(staff, created.Id, metadata, Tag(created), idempotencyKey: key));
        Assert.Equal(contentResult, await service.UpdateContentAsync(staff, created.Id, content, Tag(edited), idempotencyKey: key));
        Assert.Equal(publishResult, await otherDraft.Run(service, key, staff));
        var after = await fixture.Snapshot();
        Assert.Equal(baseline.Curricula.Count + 1, after.Curricula.Count);
        Assert.Equal(baseline.AuditEvents.Count + 4, after.AuditEvents.Count);
        Assert.Equal((baseline.Idempotency?.Count ?? 0) + 4, after.Idempotency!.Count);
        Assert.Equal(4, after.Idempotency.TakeLast(4).Select(e => e.Operation).Distinct().Count());
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task RejectedActorsCannotReadReplay(bool file, string operation)
    {
        var fixture = new CurriculumCommitSelfCheckFixture(file);
        var mutation = await fixture.Prepare(operation);
        var key = Guid.NewGuid();
        await mutation.Run(fixture.Service(), key);
        var baseline = Json(await fixture.Snapshot());
        var unauthorized = new[]
        {
            mutation.Actor with { Roles = [] },
            CurriculumPrincipalStub.CreateSchoolAdmin(101, principalId: Author.PrincipalId!),
            CurriculumPrincipalStub.CreateSchoolTeacher(102),
            mutation.Actor with { IsSessionExpired = true }
        };
        foreach (var actor in unauthorized)
        {
            var error = await Assert.ThrowsAsync<DomainRuleException>(() => mutation.Run(fixture.Service(), key, actor));
            Assert.Contains(error.Code, new[] { ErrorCodes.StaffRoleRequired, ErrorCodes.ChannelNotAllowed, ErrorCodes.SessionExpired });
            Assert.Equal(baseline, Json(await fixture.Snapshot()));
        }
    }
}
