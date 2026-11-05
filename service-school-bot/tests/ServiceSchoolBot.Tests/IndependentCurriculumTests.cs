using Grpc.Core;
using Rpc = SchoolBot.Infrastructure.Protos;
using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Interfaces.Curriculum;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

public sealed class IndependentCurriculumTests
{
    private const string Author = "token-staff-author-valid";
    private const string Approver = "token-staff-approver-valid";
    private const string School = "token-school-admin-valid";
    private const string Student = "token-student-valid";

    private static Metadata As(string token) => GrpcTest.As(token);

    private static Task<GrpcOutcome<Rpc.CurriculumMutationResponse>> UpdateContent(
        Rpc.CurriculumService.CurriculumServiceClient client, int id, string? tag, LessonInput[] lessons,
        string token = Author) =>
        GrpcTest.Try(client.UpdateCurriculumContentAsync(
            CurriculumRpc.Content(id, new UpdateCurriculumContentRequest(lessons), tag, Guid.NewGuid()), As(token)));

    private static Task<Rpc.CurriculumAdminResponse> ReadAdmin(Rpc.CurriculumService.CurriculumServiceClient client,
        int id) => client.GetAdminCurriculumAsync(new() { Id = id }, As(Author)).ResponseAsync;

    private static Task<GrpcOutcome<Rpc.CurriculumDraftResponse>> ReadPublished(
        Rpc.CurriculumService.CurriculumServiceClient client, int id, string token) =>
        GrpcTest.Try(client.GetPublishedCurriculumAsync(new() { Id = id }, As(token)));

    private static CreateCurriculumRequest Metadata() => new(Guid.NewGuid(), 1, 1, "หลักสูตร", "source:independent", ["ป.1"], []);
    private static LessonInput Lesson(int ordinal = 1) => new(Guid.NewGuid(), ordinal, "บทเรียน", "เป้าหมาย", "คำแนะนำ",
        [new MissionInput(Guid.NewGuid(), 1, "ภารกิจ", "ทำตามเกณฑ์", "teacher",
            new CriteriaInput(1, "{\"kind\":\"teacher-v1\",\"items\":[{\"id\":\"f27ceba9-d92f-4daa-bb4e-a2f865f01418\",\"instruction\":\"ตรวจงาน\"}]}", "source:criteria", "human", DateTimeOffset.UtcNow))],
        [new ActivityInput(Guid.NewGuid(), 1, "กิจกรรม", "project", "arm", "[]", "{}")]);

    private static async Task<(int Id, string Tag)> Create(Rpc.CurriculumService.CurriculumServiceClient client,
        CreateCurriculumRequest? input = null)
    {
        var response = await client.CreateCurriculumAsync(CurriculumRpc.Create(input ?? Metadata(), Guid.NewGuid()), As(Author));
        return (response.Id, response.Etag);
    }

    private static async Task<string> Content(Rpc.CurriculumService.CurriculumServiceClient client,
        int id, string tag, params LessonInput[] lessons)
    {
        var response = await client.UpdateCurriculumContentAsync(
            CurriculumRpc.Content(id, new UpdateCurriculumContentRequest(lessons), tag, Guid.NewGuid()), As(Author));
        return response.Etag;
    }

    private static Task<GrpcOutcome<Rpc.CurriculumMutationResponse>> Publish(
        Rpc.CurriculumService.CurriculumServiceClient client, int id, string tag,
        string? reference = "approval:independent", string token = Approver, Guid? key = null) =>
        GrpcTest.Try(client.PublishCurriculumAsync(CurriculumRpc.Publish(id, reference, tag, key ?? Guid.NewGuid()), As(token)));

    [Fact]
    public async Task ReorderPreservesPermanentLessonKeys()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (id, tag) = await Create(client);
        var first = Lesson();
        var second = Lesson(2);
        tag = await Content(client, id, tag, first, second);
        factory.Clock.Advance(TimeSpan.FromSeconds(1));
        await Content(client, id, tag, second with { Ordinal = 1 }, first with { Ordinal = 2 });
        var read = await ReadAdmin(client, id);
        var lessons = read.Lessons;
        Assert.Equal(second.LessonKey.ToString(), lessons[0].Lesson.LessonKey);
        Assert.Equal(first.LessonKey.ToString(), lessons[1].Lesson.LessonKey);
    }

    [Fact]
    public async Task DraftEditReadsBackNewContent()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (id, tag) = await Create(client);
        var lesson = Lesson();
        tag = await Content(client, id, tag, lesson);
        await Content(client, id, tag, lesson with { Objective = "เป้าหมายใหม่" });
        var read = await ReadAdmin(client, id);
        Assert.Equal("เป้าหมายใหม่", read.Lessons[0].Lesson.Objective);
    }

    [Fact]
    public async Task PublishRequiresEvidenceAndDifferentAuthorizedHuman()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (id, tag) = await Create(client);
        var missing = await Publish(client, id, tag, null);
        Assert.Equal(400, missing.HttpStatus);
        var noRole = await Publish(client, id, tag, token: "token-staff-norole-valid");
        Assert.Equal(403, noRole.HttpStatus);
        using var serviceScope = factory.Services.CreateScope();
        var service = serviceScope.ServiceProvider.GetRequiredService<ICurriculumService>();
        var error = await Assert.ThrowsAsync<DomainRuleException>(() => service.PublishCurriculumAsync(
            CurriculumPrincipalStub.CreateStaffApprover("staff-author-01"), id, new PublishCurriculumRequest("self"), tag));
        Assert.Equal("FOUR_EYES_REQUIRED", error.Code);
        var published = await Publish(client, id, tag);
        Assert.Equal(200, published.HttpStatus);
        var read = await ReadAdmin(client, id);
        var view = read;
        Assert.Equal("staff-approver-01", view.Approval.Actor);
        Assert.Equal("source:independent", view.Summary.SourceRef);
    }

    [Fact]
    public async Task NewRevisionLeavesOldPublishedSnapshotReadableAndImmutable()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var input = Metadata();
        var (id, tag) = await Create(client, input);
        tag = await Content(client, id, tag, Lesson());
        var published = await Publish(client, id, tag);
        Assert.Equal(200, published.HttpStatus);
        var before = await ReadPublished(client, id, Student);
        var snapshot = before.Response!.ToString();
        var (nextId, nextTag) = await Create(client, input with { Revision = 2, Title = "หลักสูตรใหม่" });
        var next = await Publish(client, nextId, nextTag, "approval:new");
        Assert.Equal(200, next.HttpStatus);
        var edit = await UpdateContent(client, id, published.Response!.Etag, [Lesson()]);
        Assert.Equal(409, edit.HttpStatus);
        var after = await ReadPublished(client, id, Student);
        Assert.Equal(snapshot, after.Response!.ToString());
    }

    [Fact]
    public async Task SchoolCanReadPublishedButCannotCreateOrEditCentralContent()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var denied = await GrpcTest.Try(client.CreateCurriculumAsync(CurriculumRpc.Create(Metadata(), Guid.NewGuid()), As(School)));
        Assert.Equal(403, denied.HttpStatus);
        var (id, tag) = await Create(client);
        var edit = await UpdateContent(client, id, tag, [Lesson()], School);
        Assert.Equal(403, edit.HttpStatus);
        var pub = await Publish(client, id, tag);
        var read = await ReadPublished(client, id, School);
        Assert.Equal(200, read.HttpStatus);
    }

    [Fact]
    public async Task DraftIsNotOfferedAsApprovedPublicContent()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (id, _) = await Create(client);
        var list = await client.ListPublishedCurriculaAsync(new(), As(Student));
        Assert.Empty(list.Items);
        var read = await ReadPublished(client, id, Student);
        Assert.Equal(403, read.HttpStatus);
    }

    [Theory]
    [InlineData("block", "automatic")]
    [InlineData("arm", "teacher")]
    [InlineData("code", "automatic")]
    public async Task PublishedMissionRetainsItsContentCriteriaAndApproval(string mode, string method)
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (id, tag) = await Create(client);
        var lesson = Lesson();
        var rules = method == "automatic"
            ? "{\"kind\":\"automatic-v1\",\"combine\":\"all\",\"rules\":[{\"id\":\"f27ceba9-d92f-4daa-bb4e-a2f865f01418\",\"type\":\"steps_at_most\",\"maximum\":10}]}"
            : lesson.Missions[0].Criteria.Rules;
        lesson = lesson with
        {
            Missions = [lesson.Missions[0] with { Method = method, Criteria = lesson.Missions[0].Criteria with { Rules = rules } }],
            Activities = [lesson.Activities[0] with { Mode = mode }]
        };
        tag = await Content(client, id, tag, lesson);
        var pub = await Publish(client, id, tag);
        Assert.Equal(200, pub.HttpStatus);
        var db = factory.Services.GetRequiredService<ISchoolBotRepositoryScope>();
        var read = await ReadPublished(client, id, Student);
        Assert.Equal(200, read.HttpStatus);
        var publicLesson = read.Response!.Lessons[0];
        Console.WriteLine($"public detail: missions={publicLesson.Missions.Count}, activities={publicLesson.Activities.Count}");
        var storedLesson = Assert.Single(await db.ListLessonsByCurriculumIdAsync(id));
        var store = factory.Services.GetRequiredService<ISchoolBotBackingStore>();
        var rawMissions = await store.LoadMissionsAsync();
        var rawCriteria = await store.LoadCriteriaAsync();
        var rawActivities = await store.LoadActivitiesAsync();
        Console.WriteLine($"Lesson PK={storedLesson.SchoolBotLessonId}; Mission.LessonId={string.Join(",", rawMissions.Select(m => m.LessonId))}; Criteria.MissionId={string.Join(",", rawCriteria.Select(c => c.MissionId))}; Activity.LessonId={string.Join(",", rawActivities.Select(a => a.LessonId))}");
        var mission = Assert.Single(await db.ListMissionsByLessonIdAsync(storedLesson.SchoolBotLessonId));
        Assert.Equal(lesson.Missions[0].MissionKey, mission.MissionKey);
        Assert.Single(await db.ListCriteriaByMissionIdAsync(mission.SchoolBotMissionId));
        Assert.Single(await db.ListActivitiesByLessonIdAsync(storedLesson.SchoolBotLessonId));
    }

    [Theory]
    [InlineData(1000, 200)]
    [InlineData(1001, 400)]
    public async Task ApprovalBoundPreservesExactTextOrRejectsWithoutWrites(int length, int status)
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (id, tag) = await Create(client);
        var reference = " " + new string('ก', length - 2) + " ";
        var response = await Publish(client, id, tag, reference);
        Assert.Equal(status, (int)response.HttpStatus);
        var db = factory.Services.GetRequiredService<ISchoolBotRepositoryScope>();
        var record = await db.GetCurriculumByIdAsync(id);
        var events = await db.ListAuditEventsByEntityAsync("SchoolBotCurriculum", id);
        if (status == 400)
        {
            Assert.Equal("draft", record!.Lifecycle);
            Assert.Single(events);
            Assert.Equal(tag, OpaqueConcurrencyToken.ToETag(record.UpdateDate));
        }
        else
        {
            Assert.Equal(reference, Assert.Single(events, e => e.Event == "Curriculum.Published").Reason);
            var read = await ReadAdmin(client, id);
            Assert.Equal(reference, read.Approval.ApprovalRef);
        }
    }

    [Theory]
    [InlineData(null, 428)]
    [InlineData("\"stale\"", 412)]
    public async Task RejectedContentPreconditionLeavesEntireStateUntouched(string? tag, int status)
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (id, originalTag) = await Create(client);
        var response = await UpdateContent(client, id, tag, [Lesson()]);
        Assert.Equal(status, (int)response.HttpStatus);
        var db = factory.Services.GetRequiredService<ISchoolBotRepositoryScope>();
        Assert.Empty(await db.ListLessonsByCurriculumIdAsync(id));
        Assert.Single(await db.ListAuditEventsByEntityAsync("SchoolBotCurriculum", id));
        Assert.Equal(originalTag, OpaqueConcurrencyToken.ToETag((await db.GetCurriculumByIdAsync(id))!.UpdateDate));
    }

    [Fact]
    public async Task CreateRetryWithSameKeyReturnsOriginalResource()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var input = Metadata();
        var key = Guid.NewGuid();
        var first = await GrpcTest.Try(client.CreateCurriculumAsync(CurriculumRpc.Create(input, key), As(Author)));
        var retry = await GrpcTest.Try(client.CreateCurriculumAsync(CurriculumRpc.Create(input, key), As(Author)));
        Assert.Equal(200, first.HttpStatus);
        Assert.Equal(200, retry.HttpStatus);
        Assert.Equal(first.Response!.Id, retry.Response!.Id);
    }

    [Fact]
    public async Task CreateSameKeyDifferentPayloadConflictsWithoutCreatingAnotherRevision()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var input = Metadata();
        var key = Guid.NewGuid();
        var first = await GrpcTest.Try(client.CreateCurriculumAsync(CurriculumRpc.Create(input, key), As(Author)));
        var changed = await GrpcTest.Try(client.CreateCurriculumAsync(CurriculumRpc.Create(input with { Revision = 2 }, key), As(Author)));
        Assert.Equal(200, first.HttpStatus);
        Console.WriteLine($"เปลี่ยน payload: HTTP={(int)changed.HttpStatus}; revisions ใน store={(await factory.Services.GetRequiredService<ISchoolBotRepositoryScope>().ListCurriculaAsync()).Count}");
        Assert.Equal(409, changed.HttpStatus);
        Assert.Single(await factory.Services.GetRequiredService<ISchoolBotRepositoryScope>().ListCurriculaAsync());
    }

    [Fact]
    public async Task PublishReplayDoesNotReturnAnotherActorsResult()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (id, tag) = await Create(client);
        var key = Guid.NewGuid();
        var first = await Publish(client, id, tag, key: key);
        Assert.Equal(200, first.HttpStatus);
        var other = await Publish(client, id, first.Response!.Etag, token: School, key: key);
        Assert.Equal(403, other.HttpStatus);
    }

    [Fact]
    public async Task PublishSameKeyDifferentPayloadReportsIdempotencyConflict()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (id, tag) = await Create(client);
        var key = Guid.NewGuid();
        var first = await Publish(client, id, tag, key: key);
        var other = await Publish(client, id, first.Response!.Etag, "different", key: key);
        Assert.Equal(409, other.HttpStatus);
        Assert.Equal("IDEMPOTENCY_CONFLICT", other.Error!.Code);
    }

    [Fact]
    public async Task TwoHostsPublishingSameStoreHaveOnlyOneWinnerAndMatchingDiskAudit()
    {
        var path = Path.Combine(Path.GetTempPath(), "IndependentCurriculum-" + Guid.NewGuid().ToString("N"));
        using var root = new ApiFactory();
        using var hostA = root.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSchoolBotRepositories(new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(path)))));
        using var hostB = root.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSchoolBotRepositories(new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(path)))));
        var clientA = GrpcTest.Curricula(hostA);
        var clientB = GrpcTest.Curricula(hostB);
        var (id, tag) = await Create(clientA);
        var warm = await ReadAdmin(clientB, id);
        Assert.NotNull(warm.Summary);
        var responses = await Task.WhenAll(Publish(clientA, id, tag, "approval:A"), Publish(clientB, id, tag, "approval:B"));
        var disk = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(path));
        var events = await disk.ListAuditEventsByEntityAsync("SchoolBotCurriculum", id);
        Assert.Single(events, e => e.Event == "Curriculum.Published");
        Console.WriteLine($"ผล publish สอง host: {(int)responses[0].HttpStatus}, {(int)responses[1].HttpStatus}; ดิสก์: {path}");
        Assert.Single(responses, r => r.HttpStatus == 200);
        Assert.Single(responses, r => r.HttpStatus is 409 or 412);
    }

    [Fact]
    public async Task RejectedPublishLeavesDiskBytesAndReloadedStateUntouched()
    {
        var path = Path.Combine(Path.GetTempPath(), "IndependentCurriculum-Reject-" + Guid.NewGuid().ToString("N"));
        using var root = new ApiFactory();
        using var host = root.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSchoolBotRepositories(new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(path)))));
        var client = GrpcTest.Curricula(host);
        var (id, tag) = await Create(client);
        tag = await Content(client, id, tag, Lesson());
        var file = Path.Combine(path, "curricula_snapshot.json");
        var before = await File.ReadAllBytesAsync(file);
        var response = await Publish(client, id, tag, new string('ก', 1001));
        Assert.Equal(400, response.HttpStatus);
        Assert.Equal(before, await File.ReadAllBytesAsync(file));
        var disk = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(path));
        Assert.Equal("draft", (await disk.GetCurriculumByIdAsync(id))!.Lifecycle);
        Assert.Equal(2, (await disk.ListAuditEventsByEntityAsync("SchoolBotCurriculum", id)).Count);
    }

    [Fact]
    public async Task UnapprovedCriteriaCannotBePublishedAsApprovedLesson()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (id, tag) = await Create(client);
        var lesson = Lesson();
        lesson = lesson with { Missions = [lesson.Missions[0] with
            { Criteria = lesson.Missions[0].Criteria with { ApprovedBy = null, ApprovedAt = null } }] };
        tag = await Content(client, id, tag, lesson);
        var response = await Publish(client, id, tag);
        Console.WriteLine($"เกณฑ์ไม่มีผู้อนุมัติ: HTTP={(int)response.HttpStatus}; lifecycle={(await factory.Services.GetRequiredService<ISchoolBotRepositoryScope>().GetCurriculumByIdAsync(id))!.Lifecycle}");
        Assert.NotEqual(200, response.HttpStatus);
        Assert.Equal("draft", (await factory.Services.GetRequiredService<ISchoolBotRepositoryScope>().GetCurriculumByIdAsync(id))!.Lifecycle);
    }

    [Fact]
    public async Task EarlierContributorCannotApproveAfterAnotherEditorUpdatesDraft()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (id, tag) = await Create(client);
        using var serviceScope = factory.Services.CreateScope();
        var service = serviceScope.ServiceProvider.GetRequiredService<ICurriculumService>();
        var edited = await service.UpdateContentAsync(CurriculumPrincipalStub.CreateStaffAuthor("other-editor"),
            id, new UpdateCurriculumContentRequest([Lesson()]), tag);
        var error = await Assert.ThrowsAsync<DomainRuleException>(() => service.PublishCurriculumAsync(
            CurriculumPrincipalStub.CreateStaffApprover("staff-author-01"), id,
            new PublishCurriculumRequest("approval:self"), OpaqueConcurrencyToken.ToETag(edited.UpdateDate)));
        Assert.Equal("FOUR_EYES_REQUIRED", error.Code);
    }
}


