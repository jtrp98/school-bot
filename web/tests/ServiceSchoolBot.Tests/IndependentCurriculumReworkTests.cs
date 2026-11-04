using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using ServiceSchoolBot.Application.Services.Interfaces.Curriculum;
using System.Reflection;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

public sealed class IndependentCurriculumReworkTests
{
    private static readonly CurriculumPrincipalContext Author = CurriculumPrincipalStub.CreateStaffAuthor();
    private static readonly CurriculumPrincipalContext Approver = CurriculumPrincipalStub.CreateStaffApprover();
    private static CreateCurriculumRequest Metadata() => new(Guid.NewGuid(), 1, 1, "หลักสูตร", "source:independent", ["ป.1"], []);
    private static string Tag(CurriculumMutationResponse response) => OpaqueConcurrencyToken.ToETag(response.UpdateDate);
    private static (CurriculumService Service, SchoolBotUnitOfWork Db, string Path) Setup()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "IndependentCurriculum-Rework-" + Guid.NewGuid().ToString("N"));
        var db = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(path));
        return (new CurriculumService(db, TimeProvider.System), db, path);
    }

    private static LessonInput Lesson(int ordinal = 1) => new(Guid.NewGuid(), ordinal, "บทเรียน", "เป้าหมาย", "คำแนะนำ",
        [new MissionInput(Guid.NewGuid(), 1, "ภารกิจ", "ตรวจงาน", "teacher",
            new CriteriaInput(1, "{\"kind\":\"teacher-v1\",\"items\":[{\"id\":\"f27ceba9-d92f-4daa-bb4e-a2f865f01418\",\"instruction\":\"ตรวจงาน\"}]}", "source:criteria", "human", DateTimeOffset.UtcNow))],
        [new ActivityInput(Guid.NewGuid(), 1, "กิจกรรม", "project", "arm", "[]", "{}")]);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PartiallyApprovedCriteriaCannotPublish(bool missingActor)
    {
        var (service, _, path) = Setup();
        var created = await service.CreateCurriculumAsync(Author, Metadata());
        var lesson = Lesson();
        var criteria = lesson.Missions[0].Criteria;
        lesson = lesson with { Missions = [lesson.Missions[0] with { Criteria = missingActor
            ? criteria with { ApprovedBy = " " } : criteria with { ApprovedAt = null } }] };
        var edited = await service.UpdateContentAsync(Author, created.Id, new UpdateCurriculumContentRequest([lesson]), Tag(created));
        var before = await File.ReadAllBytesAsync(System.IO.Path.Combine(path, "curricula_snapshot.json"));
        var error = await Assert.ThrowsAsync<DomainRuleException>(() => service.PublishCurriculumAsync(
            Approver, created.Id, new PublishCurriculumRequest("approval:partial"), Tag(edited)));
        Assert.Equal("CRITERIA_NOT_APPROVED", error.Code);
        Assert.Equal(before, await File.ReadAllBytesAsync(System.IO.Path.Combine(path, "curricula_snapshot.json")));
    }

    [Fact]
    public async Task ContentRetryReturnsOriginalResultWithoutReplacingChildrenAgain()
    {
        var (service, db, path) = Setup();
        var created = await service.CreateCurriculumAsync(Author, Metadata());
        var input = new UpdateCurriculumContentRequest([Lesson()]);
        var key = Guid.NewGuid();
        var first = await service.UpdateContentAsync(Author, created.Id, input, Tag(created), idempotencyKey: key);
        var before = await File.ReadAllBytesAsync(System.IO.Path.Combine(path, "curricula_snapshot.json"));
        var retry = await service.UpdateContentAsync(Author, created.Id, input, Tag(created), idempotencyKey: key);
        Assert.Equal(first, retry);
        Assert.Equal(before, await File.ReadAllBytesAsync(System.IO.Path.Combine(path, "curricula_snapshot.json")));
        Assert.Equal(2, (await db.ListAuditEventsByEntityAsync("SchoolBotCurriculum", created.Id)).Count);
    }

    [Fact]
    public async Task MultipleLessonsRetainTheirOwnChildrenAfterReplacementAndReload()
    {
        var (service, _, path) = Setup();
        var created = await service.CreateCurriculumAsync(Author, Metadata());
        var first = Lesson();
        var second = Lesson(2);
        var edited = await service.UpdateContentAsync(Author, created.Id, new UpdateCurriculumContentRequest([first, second]), Tag(created));
        await service.UpdateContentAsync(Author, created.Id,
            new UpdateCurriculumContentRequest([second with { Ordinal = 1 }, first with { Ordinal = 2 }]), Tag(edited));
        var disk = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(path));
        var rows = await disk.ListLessonsByCurriculumIdAsync(created.Id);
        Assert.Equal(2, rows.Count);
        foreach (var expected in new[] { first, second })
        {
            var actual = Assert.Single(rows, row => row.LessonKey == expected.LessonKey);
            var mission = Assert.Single(await disk.ListMissionsByLessonIdAsync(actual.SchoolBotLessonId));
            Assert.Equal(expected.Missions[0].MissionKey, mission.MissionKey);
            Assert.Single(await disk.ListCriteriaByMissionIdAsync(mission.SchoolBotMissionId));
            Assert.Equal(expected.Activities[0].ActivityKey,
                Assert.Single(await disk.ListActivitiesByLessonIdAsync(actual.SchoolBotLessonId)).ActivityKey);
        }
    }
    private static Task<CurriculumMutationResponse> Mutate(ICurriculumService service, string operation,
        CurriculumPrincipalContext actor, CreateCurriculumRequest input, CurriculumMutationResponse created, Guid key) => operation switch
    {
        "create" => service.CreateCurriculumAsync(actor, input, idempotencyKey: key),
        "metadata" => service.UpdateMetadataAsync(actor, created.Id,
            new CurriculumMetadataInput(1, "ชื่อใหม่", "source:edit", ["ป.1"], []), Tag(created), idempotencyKey: key),
        "content" => service.UpdateContentAsync(actor, created.Id, new UpdateCurriculumContentRequest([]), Tag(created), idempotencyKey: key),
        "publish" => service.PublishCurriculumAsync(actor, created.Id, new PublishCurriculumRequest("approval:rework"), Tag(created), idempotencyKey: key),
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };

    [Theory]
    [InlineData("create")]
    [InlineData("metadata")]
    [InlineData("content")]
    [InlineData("publish")]
    public async Task AuthorizedActorCannotReceiveAnotherActorsReplay(string operation)
    {
        var (service, db, path) = Setup();
        var input = Metadata();
        var created = operation == "create" ? new CurriculumMutationResponse(0, DateTimeOffset.UtcNow)
            : await service.CreateCurriculumAsync(Author, input);
        var key = Guid.NewGuid();
        await Mutate(service, operation, operation == "publish" ? Approver : Author, input, created, key);
        var before = await File.ReadAllBytesAsync(System.IO.Path.Combine(path, "curricula_snapshot.json"));
        var other = operation == "publish" ? CurriculumPrincipalStub.CreateStaffApprover("other-approver")
            : CurriculumPrincipalStub.CreateStaffAuthor("other-author");
        var error = await Assert.ThrowsAsync<DomainRuleException>(() => operation == "publish"
            ? service.PublishCurriculumAsync(other, created.Id, new PublishCurriculumRequest("approval:other"),
                Tag(created), idempotencyKey: key)
            : Mutate(service, operation, other, input, created, key));
        Assert.Equal(operation is "metadata" or "content" ? "VERSION_CONFLICT" : "STATE_CONFLICT", error.Code);
        Assert.Equal(before, await File.ReadAllBytesAsync(System.IO.Path.Combine(path, "curricula_snapshot.json")));
        Assert.Single(await db.ListCurriculaAsync());
    }

    [Theory]
    [InlineData("create")]
    [InlineData("content")]
    public async Task SchoolCannotReplaySuccessfulCentralMutation(string operation)
    {
        var (service, _, path) = Setup();
        var input = Metadata();
        var created = operation == "create" ? new CurriculumMutationResponse(0, DateTimeOffset.UtcNow)
            : await service.CreateCurriculumAsync(Author, input);
        var key = Guid.NewGuid();
        await Mutate(service, operation, Author, input, created, key);
        var before = await File.ReadAllBytesAsync(System.IO.Path.Combine(path, "curricula_snapshot.json"));
        var error = await Assert.ThrowsAsync<DomainRuleException>(() =>
            Mutate(service, operation, CurriculumPrincipalStub.CreateSchoolAdmin(), input, created, key));
        Assert.Contains(error.Code, new[] { "CHANNEL_NOT_ALLOWED", "FORBIDDEN", "STAFF_ROLE_REQUIRED" });
        Assert.Equal(before, await File.ReadAllBytesAsync(System.IO.Path.Combine(path, "curricula_snapshot.json")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConcurrentCreateReservesKeyBeforeCommitting(bool differentPayload)
    {
        var (_, _, path) = Setup();
        var proxy = DispatchProxy.Create<ISchoolBotRepositoryScope, CurriculumGateProxy>();
        var gate = (CurriculumGateProxy)(object)proxy;
        gate.Target = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(path));
        gate.Operation = nameof(ISchoolBotRepositoryScope.CreateCurriculumAsync);
        var firstService = new CurriculumService(proxy, TimeProvider.System);
        var secondProxy = DispatchProxy.Create<ISchoolBotRepositoryScope, CurriculumGateProxy>();
        var secondGate = (CurriculumGateProxy)(object)secondProxy;
        secondGate.Target = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(path));
        secondGate.ObserveOnly = true;
        var secondService = new CurriculumService(secondProxy, TimeProvider.System);
        var input = Metadata();
        var key = Guid.NewGuid();
        var first = firstService.CreateCurriculumAsync(Author, input, idempotencyKey: key);
        await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        CurriculumMutationResponse? secondResult = null;
        Exception? secondError;
        try
        {
            var second = secondService.CreateCurriculumAsync(
                Author, differentPayload ? input with { Revision = 2 } : input, idempotencyKey: key);
            await secondGate.CommitEntered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.False(second.IsCompleted);
            gate.Release.TrySetResult();
            secondError = await Record.ExceptionAsync(async () =>
                secondResult = await second.WaitAsync(TimeSpan.FromSeconds(10)));
        }
        finally { gate.Release.TrySetResult(); }
        CurriculumMutationResponse? firstResult = null;
        var firstError = await Record.ExceptionAsync(async () => firstResult = await first);
        var disk = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(path));
        var rows = await disk.ListCurriculaAsync();
        Console.WriteLine($"concurrent key differentPayload={differentPayload}; first={firstError?.Message ?? "success"}; second={secondError?.Message ?? "success"}; disk revisions={rows.Count}; path={path}");
        var persisted = await new FileSchoolBotBackingStore(path).LoadCurriculumUnitAsync();
        Assert.Single(persisted.AuditEvents);
        Assert.Single(persisted.Idempotency!);
        if (differentPayload)
        {
            Assert.Single(rows);
            Assert.Null(firstError);
            Assert.Equal("IDEMPOTENCY_CONFLICT", Assert.IsType<DomainRuleException>(secondError).Code);
        }
        else
        {
            Assert.Null(firstError);
            Assert.Null(secondError);
            Assert.Equal(firstResult, secondResult);
            Assert.Single(rows);
        }
    }

    [Fact]
    public async Task PendingPublishPreservesAnotherHostsNewRevision()
    {
        var (service, _, path) = Setup();
        var input = Metadata();
        var created = await service.CreateCurriculumAsync(Author, input);
        var proxy = DispatchProxy.Create<ISchoolBotRepositoryScope, CurriculumGateProxy>();
        var gate = (CurriculumGateProxy)(object)proxy;
        gate.Target = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(path));
        gate.Operation = nameof(ISchoolBotRepositoryScope.PublishCurriculumAsync);
        gate.PauseBefore = true;
        var publishing = new CurriculumService(proxy, TimeProvider.System).PublishCurriculumAsync(
            Approver, created.Id, new PublishCurriculumRequest("approval:pending"), Tag(created));
        await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        CurriculumMutationResponse next;
        try { next = await service.CreateCurriculumAsync(Author, input with { Revision = 2 }); }
        finally { gate.Release.TrySetResult(); }
        await publishing;
        var disk = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(path));
        Assert.Equal(2, (await disk.ListCurriculaAsync()).Count);
        Assert.Equal("published", (await disk.GetCurriculumByIdAsync(created.Id))!.Lifecycle);
        Assert.Equal("draft", (await disk.GetCurriculumByIdAsync(next.Id))!.Lifecycle);
        Assert.Equal(2, (await disk.ListAuditEventsByEntityAsync("SchoolBotCurriculum", created.Id)).Count);
        Assert.Single(await disk.ListAuditEventsByEntityAsync("SchoolBotCurriculum", next.Id));
    }
}

public class CurriculumGateProxy : DispatchProxy, IIdempotentSchoolBotCommit
{
    public ISchoolBotRepositoryScope Target { get; set; } = null!;
    public string Operation { get; set; } = "";
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public bool PauseBefore { get; set; }
    public bool ObserveOnly { get; set; }
    public TaskCompletionSource CommitEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public async Task<IdempotentCommitResult<T>> CommitWithIdempotencyAsync<T>(IdempotencyCommand command,
        Func<ISchoolBotRepositoryScope, int?, DateTimeOffset, CancellationToken, Task> recheckAccess,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<MutationResult<T>>> mutate,
        CancellationToken ct = default)
    {
        CommitEntered.TrySetResult();
        if (PauseBefore)
        {
            Entered.TrySetResult();
            await Release.Task.WaitAsync(TimeSpan.FromSeconds(10), ct);
        }
        return await ((IIdempotentSchoolBotCommit)Target).CommitWithIdempotencyAsync(command, recheckAccess,
            async (facade, now, token) =>
            {
                if (!PauseBefore && !ObserveOnly)
                {
                    Entered.TrySetResult();
                    await Release.Task.WaitAsync(TimeSpan.FromSeconds(10), token);
                }
                return await mutate(facade, now, token);
            }, ct);
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod!.Name == Operation && !Entered.Task.IsCompleted)
            return Delay(targetMethod, args);
        return targetMethod.Invoke(Target, args);
    }

    private async Task<SchoolBotCurriculumRecord> Delay(MethodInfo method, object?[]? args)
    {
        Entered.TrySetResult();
        await Release.Task.WaitAsync(TimeSpan.FromSeconds(15));
        return await (Task<SchoolBotCurriculumRecord>)method.Invoke(Target, args)!;
    }
}

