using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using System.Reflection;
using System.Text.Json;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

internal sealed class CurriculumCommitSelfCheckFixture
{
    public static readonly CurriculumPrincipalContext Author = CurriculumPrincipalStub.CreateStaffAuthor();
    public static readonly CurriculumPrincipalContext Editor = CurriculumPrincipalStub.CreateStaffAuthor("editor");
    public static readonly CurriculumPrincipalContext Approver = CurriculumPrincipalStub.CreateStaffApprover();
    public static readonly CurriculumPrincipalContext SecondApprover = CurriculumPrincipalStub.CreateStaffApprover("approver-two");
    private readonly MemorySchoolBotBackingStore memory = new();
    private readonly string? directory;
    public SelfCheckClock Clock { get; } = new();
    public CurriculumCommitSelfCheckFixture(bool file)
    {
        if (file) directory = Path.Combine(Path.GetTempPath(), "CurriculumCommitSelfCheck-" + Guid.NewGuid().ToString("N"));
    }

    public ISchoolBotBackingStore Store() => directory is null ? memory : new FileSchoolBotBackingStore(directory);
    public SchoolBotUnitOfWork Adapter() => new(Store(), Clock);
    public CurriculumService Service() => new(Adapter(), Clock);
    public (CurriculumService Service, CurriculumCommitSelfCheckProxy Gate) Gated()
    {
        var proxy = DispatchProxy.Create<ISchoolBotRepositoryScope, CurriculumCommitSelfCheckProxy>();
        var gate = (CurriculumCommitSelfCheckProxy)(object)proxy;
        gate.Target = Adapter();
        return (new CurriculumService(proxy, Clock), gate);
    }

    public async Task<CurriculumStoreSnapshot> Snapshot()
    {
        var store = Store();
        var snapshot = await ((ICurriculumBackingStore)store).LoadCurriculumUnitAsync();
        var reader = new SchoolBotUnitOfWork(store, Clock);
        var rows = await reader.ListCurriculaAsync();
        Assert.Equal(snapshot.Curricula.Count, rows.Count);
        foreach (var row in rows)
        {
            var lessons = await reader.ListLessonsByCurriculumIdAsync(row.SchoolBotCurriculumId);
            Assert.Equal(snapshot.Lessons.Count(lesson => lesson.CurriculumId == row.SchoolBotCurriculumId), lessons.Count);
            foreach (var lesson in lessons)
            {
                var missions = await reader.ListMissionsByLessonIdAsync(lesson.SchoolBotLessonId);
                Assert.Equal(snapshot.Missions.Count(mission => mission.LessonId == lesson.SchoolBotLessonId), missions.Count);
                foreach (var mission in missions)
                    Assert.Equal(snapshot.Criteria.Count(criteria => criteria.MissionId == mission.SchoolBotMissionId),
                        (await reader.ListCriteriaByMissionIdAsync(mission.SchoolBotMissionId)).Count);
                Assert.Equal(snapshot.Activities.Count(activity => activity.LessonId == lesson.SchoolBotLessonId),
                    (await reader.ListActivitiesByLessonIdAsync(lesson.SchoolBotLessonId)).Count);
            }
            Assert.Equal(snapshot.AuditEvents.Count(audit => audit.EntityId == row.SchoolBotCurriculumId),
                (await reader.ListAuditEventsByEntityAsync("SchoolBotCurriculum", row.SchoolBotCurriculumId)).Count);
        }
        return snapshot;
    }
    public static string Json(CurriculumStoreSnapshot snapshot) => JsonSerializer.Serialize(snapshot);
    public static string Tag(CurriculumMutationResponse response) => OpaqueConcurrencyToken.ToETag(response.UpdateDate);
    public static CreateCurriculumRequest Metadata() => new(Guid.NewGuid(), 1, 1, "curriculum", "source:fixture", ["P1"], []);
    public static UpdateCurriculumContentRequest Content() => new(Enumerable.Range(1, 2).Select(ordinal =>
        new LessonInput(Guid.NewGuid(), ordinal, "lesson", "objective", "instructions",
            new[] { "block", "arm", "code" }.Select((mode, index) => new MissionInput(Guid.NewGuid(), index + 1,
                mode, "instructions", "teacher", new CriteriaInput(1, "{}", "source:criteria", "human",
                    DateTimeOffset.Parse("2026-11-03T00:00:00Z")))).ToArray(),
            new[] { "block", "arm", "code" }.Select((mode, index) => new ActivityInput(Guid.NewGuid(), index + 1,
                mode, "project", mode, "[]", "{}")).ToArray())).ToArray());

    public async Task<MutationCase> Prepare(string operation, CurriculumPrincipalContext? author = null)
    {
        var input = Metadata();
        var content = Content();
        var created = new CurriculumMutationResponse(0, Clock.GetUtcNow());
        if (operation != "create") created = await Service().CreateCurriculumAsync(author ?? Author, input);
        if (operation is "content" or "publish")
            created = await Service().UpdateContentAsync(author ?? Author, created.Id, content, Tag(created));
        Clock.Now = Clock.Now.AddTicks(1);
        return new MutationCase(operation, input, content, created);
    }

    public sealed record MutationCase(string Operation, CreateCurriculumRequest Input,
        UpdateCurriculumContentRequest ContentInput, CurriculumMutationResponse Created)
    {
        public CurriculumPrincipalContext Actor => Operation == "publish" ? Approver : Author;
        public Task<CurriculumMutationResponse> Run(CurriculumService service, Guid key,
            CurriculumPrincipalContext? actor = null, bool different = false, CancellationToken ct = default,
            CurriculumMutationResponse? current = null)
        {
            var target = current ?? Created;
            return Operation switch
            {
                "create" => service.CreateCurriculumAsync(actor ?? Actor,
                    different ? Input with { Revision = 2 } : Input, idempotencyKey: key, ct: ct),
                "metadata" => service.UpdateMetadataAsync(actor ?? Actor, target.Id,
                    new CurriculumMetadataInput(1, different ? "different" : "updated", "source:updated", ["P1"], []),
                    Tag(target), idempotencyKey: key, ct: ct),
                "content" => service.UpdateContentAsync(actor ?? Actor, target.Id, different
                    ? ContentInput with { Lessons = [ContentInput.Lessons[0], ContentInput.Lessons[1] with { Objective = "different" }] }
                    : ContentInput, Tag(target), idempotencyKey: key, ct: ct),
                "publish" => service.PublishCurriculumAsync(actor ?? Actor, target.Id,
                    new PublishCurriculumRequest(different ? "approval:different" : "approval:first"),
                    Tag(target), idempotencyKey: key, ct: ct),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}

public sealed class SelfCheckClock : TimeProvider
{
    public DateTimeOffset Now { get; set; } = DateTimeOffset.Parse("2026-11-03T12:00:00Z");
    public override DateTimeOffset GetUtcNow() => Now;
}

public class CurriculumCommitSelfCheckProxy : DispatchProxy, IIdempotentSchoolBotCommit
{
    public SchoolBotUnitOfWork Target { get; set; } = null!;
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Inside { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public bool PauseInside { get; set; }
    public bool PauseBefore { get; set; }
    public bool FailAfterMutation { get; set; }
    public bool LoseResponse { get; set; }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => targetMethod!.Invoke(Target, args);

    public async Task<IdempotentCommitResult<T>> CommitWithIdempotencyAsync<T>(IdempotencyCommand command,
        Func<ISchoolBotRepositoryScope, int?, DateTimeOffset, CancellationToken, Task> recheckAccess,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<MutationResult<T>>> mutate,
        CancellationToken ct = default)
    {
        Entered.TrySetResult();
        if (PauseBefore) await Release.Task.WaitAsync(TimeSpan.FromSeconds(10), ct);
        var result = await Target.CommitWithIdempotencyAsync(command, recheckAccess, async (facade, now, token) =>
        {
            Inside.TrySetResult();
            if (PauseInside) await Release.Task.WaitAsync(TimeSpan.FromSeconds(10), token);
            var candidate = await mutate(facade, now, token);
            if (FailAfterMutation) throw new IOException("Injected before encode/swap/replace.");
            return candidate;
        }, ct);
        if (LoseResponse) throw new IOException("Injected response loss after commit.");
        return result;
    }
}
