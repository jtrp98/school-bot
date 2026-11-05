using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.Tests;

public sealed class CurriculumLifecyclePublishTests
{
    private const string StaffAuthorToken = "token-staff-author-valid";
    private const string StaffApproverToken = "token-staff-approver-valid";
    private const string SchoolAdminToken = "token-school-admin-valid";
    private const string StudentToken = "token-student-valid";

    private static Task<Rpc.CurriculumMutationResponse> Create(
        Rpc.CurriculumService.CurriculumServiceClient client, CreateCurriculumRequest body) =>
        client.CreateCurriculumAsync(CurriculumRpc.Create(body), GrpcTest.As(StaffAuthorToken)).ResponseAsync;

    private static Task<Rpc.CurriculumMutationResponse> Content(
        Rpc.CurriculumService.CurriculumServiceClient client, int id,
        UpdateCurriculumContentRequest body, string? etag) =>
        client.UpdateCurriculumContentAsync(CurriculumRpc.Content(id, body, etag),
            GrpcTest.As(StaffAuthorToken)).ResponseAsync;

    private static Task<Rpc.CurriculumMutationResponse> Publish(
        Rpc.CurriculumService.CurriculumServiceClient client, int id, string? approval, string? etag) =>
        client.PublishCurriculumAsync(CurriculumRpc.Publish(id, approval, etag),
            GrpcTest.As(StaffApproverToken)).ResponseAsync;

    private static async Task<CurriculumAdminView> ReadAdmin(
        Rpc.CurriculumService.CurriculumServiceClient client, int id) =>
        CurriculumRpc.ToModel(await client.GetAdminCurriculumAsync(new() { Id = id }, GrpcTest.As(StaffAuthorToken)));

    private static async Task<CurriculumDraftView> ReadPublished(
        Rpc.CurriculumService.CurriculumServiceClient client, int id) =>
        CurriculumRpc.ToModel(await client.GetPublishedCurriculumAsync(new() { Id = id }, GrpcTest.As(StudentToken)));

    private static (CreateCurriculumRequest CreateReq, UpdateCurriculumContentRequest ContentReq) CreateSampleCurriculumData(
        Guid curriculumKey, int revision = 1)
    {
        var row1 = new CoursePlanSuppliedRow(
            SourcePosition: 1,
            SourceRowKey: Guid.NewGuid(),
            GradeLevel: "ม.1",
            LearningConcept: "Robotics Basics",
            UnitNumber: "Unit 1",
            SourceApproved: true,
            Topic: "Introduction to Robots",
            Periods: 2,
            Details: "Fundamental robotics concepts",
            SlideSet1: "https://schoolbright.co/slides/1",
            ExerciseSet1Details: "Robotics terminology quiz"
        );

        var row2 = new CoursePlanSuppliedRow(
            SourcePosition: 2,
            SourceRowKey: Guid.NewGuid(),
            GradeLevel: "ม.1",
            LearningConcept: "Motor Controls",
            UnitNumber: "Unit 2",
            SourceApproved: true,
            Topic: "Motor Actuation",
            Periods: 2,
            Details: "Servo and DC motor controls",
            SlideSet1: "https://schoolbright.co/slides/2"
        );

        var transformResult = CoursePlanSuppliedRowTransformer.ValidateAndTransform(new[] { row1, row2 });
        Assert.True(transformResult.IsValid);

        var lessons = transformResult.Lessons.Select(l => new LessonInput(
            LessonKey: l.LessonKey,
            Ordinal: l.SourcePosition,
            Title: l.Title ?? "Untitled Lesson",
            Objective: l.LearningConcept ?? "Core concept",
            Instructions: l.Details ?? "Read guidelines",
            Missions: new[]
            {
                new MissionInput(
                    MissionKey: Guid.NewGuid(),
                    Ordinal: 1,
                    Title: $"{l.Title} Mission",
                    Instructions: "Complete robot actuation task",
                    Method: "auto",
                    Criteria: new CriteriaInput(1, "{\"pass_score\": 70}", "https://schoolbright.co/criteria/1", "approver-01", DateTimeOffset.UtcNow)
                )
            },
            Activities: new[]
            {
                new ActivityInput(
                    ActivityKey: Guid.NewGuid(),
                    Ordinal: 1,
                    Title: $"{l.Title} Arm Activity",
                    Type: "project",
                    Mode: "arm",
                    Indicators: "[]",
                    Config: "{}"
                )
            }
        )).ToList();

        var createReq = new CreateCurriculumRequest(
            CurriculumKey: curriculumKey,
            Revision: revision,
            LevelId: 1,
            Title: "Fundamental Robotics Level 1",
            SourceRef: "https://schoolbright.co/curriculum/robotics-1",
            RecommendedGrades: new[] { "ม.1", "ม.2" },
            Subjects: new[] { new CurriculumSubject(Guid.NewGuid(), "ROB-101", "Robotics 1") }
        );

        var contentReq = new UpdateCurriculumContentRequest(lessons);
        return (createReq, contentReq);
    }

    [Fact]
    public async Task CentralStaffAuthorCreatesDraftAndUpdatesContent_DraftLifecycleVerified()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (createReq, contentReq) = CreateSampleCurriculumData(Guid.NewGuid());
        var created = await Create(client, createReq);
        factory.Clock.Advance(TimeSpan.FromSeconds(1));
        var content = await Content(client, created.Id, contentReq, created.Etag);
        Assert.NotEqual(created.Etag, content.Etag);
        var view = await ReadAdmin(client, created.Id);
        Assert.Equal("draft", view.Lifecycle);
        Assert.Null(view.Approval);
        Assert.Equal(2, view.Lessons.Count);
    }

    [Fact]
    public async Task PublishCurriculumWithFourEyesAndVerbatimApprovalRef_SucceedsAndReadsBack()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (createReq, contentReq) = CreateSampleCurriculumData(Guid.NewGuid());
        var created = await Create(client, createReq);
        var content = await Content(client, created.Id, contentReq, created.Etag);
        const string approvalRef = "MOE-ACT-2026-11-APPROVAL-CURRICULUM-001";
        var published = await Publish(client, created.Id, approvalRef, content.Etag);
        Assert.True(published.HasPublishedAt);
        var view = await ReadAdmin(client, created.Id);
        Assert.Equal("published", view.Lifecycle);
        Assert.NotNull(view.Approval);
        Assert.Equal(approvalRef, view.Approval.ApprovalRef);
        Assert.Equal("staff-approver-01", view.Approval.Actor);
        Assert.Equal(approvalRef, (await ReadPublished(client, created.Id)).Approval!.ApprovalRef);
    }

    [Fact]
    public async Task Rejection_MissingApprovalRef_Returns400InvalidInput()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (createReq, _) = CreateSampleCurriculumData(Guid.NewGuid());
        var created = await Create(client, createReq);
        await GrpcTest.AssertError(client.PublishCurriculumAsync(
            CurriculumRpc.Publish(created.Id, null, created.Etag), GrpcTest.As(StaffApproverToken)),
            400, ErrorCodes.InvalidInput);
    }

    [Fact]
    public async Task Rejection_ApprovalRef1001Characters_Returns400InvalidInput()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (createReq, _) = CreateSampleCurriculumData(Guid.NewGuid());
        var created = await Create(client, createReq);
        await GrpcTest.AssertError(client.PublishCurriculumAsync(
            CurriculumRpc.Publish(created.Id, new string('A', 1001), created.Etag), GrpcTest.As(StaffApproverToken)),
            400, ErrorCodes.InvalidInput);
    }

    [Fact]
    public async Task Rejection_ApproverEqualsAuthorOrEditor_RejectedUnderFourEyesRule()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (createReq, _) = CreateSampleCurriculumData(Guid.NewGuid());
        var created = await Create(client, createReq);
        var error = await GrpcTest.Fails(client.PublishCurriculumAsync(
            CurriculumRpc.Publish(created.Id, "APPROVAL-001", created.Etag),
            GrpcTest.As(StaffAuthorToken, ("X-Test-Role", "content_approver"))));
        Assert.True(error.HttpStatus is 409 or 403);
        Assert.Equal(ErrorCodes.FourEyesRequired, error.Code);
    }

    [Fact]
    public async Task Rejection_SchoolAdminAttemptsToModifyCentralCurriculum_Returns403ChannelNotAllowed()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (createReq, contentReq) = CreateSampleCurriculumData(Guid.NewGuid());
        await GrpcTest.AssertError(client.CreateCurriculumAsync(
            CurriculumRpc.Create(createReq), GrpcTest.As(SchoolAdminToken)), 403, ErrorCodes.ChannelNotAllowed);
        var created = await Create(client, createReq);
        await GrpcTest.AssertError(client.UpdateCurriculumContentAsync(
            CurriculumRpc.Content(created.Id, contentReq, created.Etag), GrpcTest.As(SchoolAdminToken)),
            403, ErrorCodes.ChannelNotAllowed);
        await GrpcTest.AssertError(client.PublishCurriculumAsync(
            CurriculumRpc.Publish(created.Id, "REF", created.Etag), GrpcTest.As(SchoolAdminToken)),
            403, ErrorCodes.ChannelNotAllowed);
    }

    [Fact]
    public async Task Rejection_ETagCollision_Returns412VersionConflictWithCurrentETag()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (createReq, contentReq) = CreateSampleCurriculumData(Guid.NewGuid());
        var created = await Create(client, createReq);
        var error = await GrpcTest.AssertError(client.UpdateCurriculumContentAsync(
            CurriculumRpc.Content(created.Id, contentReq, "\"00000000000000000000000000000000\""),
            GrpcTest.As(StaffAuthorToken)), 412, ErrorCodes.VersionConflict);
        Assert.NotNull(error.Details);
        Assert.True(error.Details.HasCurrentEtag);
    }

    [Fact]
    public async Task Rejection_MissingIfMatch_Returns428PreconditionRequired()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (createReq, contentReq) = CreateSampleCurriculumData(Guid.NewGuid());
        var created = await Create(client, createReq);
        await GrpcTest.AssertError(client.UpdateCurriculumContentAsync(
            CurriculumRpc.Content(created.Id, contentReq, null), GrpcTest.As(StaffAuthorToken)),
            428, ErrorCodes.PreconditionRequired);
    }

    [Fact]
    public async Task PublishRetry_WithSameApprovalRef_DoesNotCreateDuplicateAuditEvent()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (createReq, _) = CreateSampleCurriculumData(Guid.NewGuid());
        var created = await Create(client, createReq);
        const string approvalRef = "APPROVAL-IDEMPOTENT-001";
        var published = await Publish(client, created.Id, approvalRef, created.Etag);
        await Publish(client, created.Id, approvalRef, published.Etag);
        var db = factory.Services.GetRequiredService<ISchoolBotRepositoryScope>();
        Assert.Single(await db.ListAuditEventsByEntityAsync("SchoolBotCurriculum", created.Id),
            audit => audit.Event == "Curriculum.Published");
    }

    [Fact]
    public async Task Ac04401_ReorderingLessonsViaDataPreservesLessonKeyAndUpdatesOrdinal()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (createReq, contentReq) = CreateSampleCurriculumData(Guid.NewGuid());
        var created = await Create(client, createReq);
        var content = await Content(client, created.Id, contentReq, created.Etag);
        var lessonA = contentReq.Lessons[0];
        var lessonB = contentReq.Lessons[1];
        await Content(client, created.Id, new([
            lessonB with { Ordinal = 1 }, lessonA with { Ordinal = 2 }]), content.Etag);
        var lessons = (await ReadAdmin(client, created.Id)).Lessons;
        Assert.Equal(lessonB.LessonKey, lessons[0].LessonKey);
        Assert.Equal(1, lessons[0].Ordinal);
        Assert.Equal(lessonA.LessonKey, lessons[1].LessonKey);
        Assert.Equal(2, lessons[1].Ordinal);
    }

    [Fact]
    public async Task Ac04402_UpdatingDraftContentReflectsImmediatelyWithoutChangingCode()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (createReq, contentReq) = CreateSampleCurriculumData(Guid.NewGuid());
        var created = await Create(client, createReq);
        var content = await Content(client, created.Id, contentReq, created.Etag);
        var updated = contentReq.Lessons[0] with { Title = "Brand New Lesson Title", Objective = "Updated Objective" };
        await Content(client, created.Id, new([updated]), content.Etag);
        var lesson = (await ReadAdmin(client, created.Id)).Lessons[0];
        Assert.Equal("Brand New Lesson Title", lesson.Title);
        Assert.Equal("Updated Objective", lesson.Objective);
    }

    [Fact]
    public async Task Ac04404_And_Ac04405_PublishedRevisionIsImmutable_NewRevisionCanBeCreatedAndPublishedSeparately()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var curriculumKey = Guid.NewGuid();
        var (createReq1, contentReq1) = CreateSampleCurriculumData(curriculumKey, 1);
        var created1 = await Create(client, createReq1);
        var content1 = await Content(client, created1.Id, contentReq1, created1.Etag);
        var published1 = await Publish(client, created1.Id, "APPROVAL-REV-1", content1.Etag);
        await GrpcTest.AssertError(client.UpdateCurriculumContentAsync(
            CurriculumRpc.Content(created1.Id, contentReq1, published1.Etag), GrpcTest.As(StaffAuthorToken)),
            409, ErrorCodes.StateConflict);
        var (createReq2, contentReq2) = CreateSampleCurriculumData(curriculumKey, 2);
        var created2 = await Create(client, createReq2 with { Title = "Robotics Level 1 (Rev 2)" });
        var content2 = await Content(client, created2.Id, contentReq2, created2.Etag);
        await Publish(client, created2.Id, "APPROVAL-REV-2", content2.Etag);
        var view1 = await ReadPublished(client, created1.Id);
        Assert.Equal(1, view1.Revision);
        Assert.Equal("Fundamental Robotics Level 1", view1.Title);
        Assert.Equal("APPROVAL-REV-1", view1.Approval!.ApprovalRef);
        var view2 = await ReadPublished(client, created2.Id);
        Assert.Equal(2, view2.Revision);
        Assert.Equal("Robotics Level 1 (Rev 2)", view2.Title);
        Assert.Equal("APPROVAL-REV-2", view2.Approval!.ApprovalRef);
    }

    [Fact]
    public async Task Ac03705_DraftCurriculumNotOfferedInPublicListing_OnlyPublishedVisible()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Curricula(factory);
        var (draftReq, _) = CreateSampleCurriculumData(Guid.NewGuid());
        var draft = await Create(client, draftReq);
        var (publishedReq, contentReq) = CreateSampleCurriculumData(Guid.NewGuid());
        var created = await Create(client, publishedReq);
        var content = await Content(client, created.Id, contentReq, created.Etag);
        await Publish(client, created.Id, "REF-001", content.Etag);
        var list = await client.ListPublishedCurriculaAsync(new(), GrpcTest.As(StudentToken));
        Assert.Contains(list.Items, item => item.Id == created.Id);
        Assert.DoesNotContain(list.Items, item => item.Id == draft.Id);
    }

    [Fact]
    public async Task PersistenceRestart_RetainsCurriculumAndApprovalAcrossRestarts()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "SchoolBotCurriculumTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var backingStore1 = new FileSchoolBotBackingStore(tempDir);
            var adapter1 = new SchoolBotUnitOfWork(backingStore1);

            var currKey = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var record = new SchoolBotCurriculumRecord(
                SchoolBotCurriculumId: 0,
                CurriculumKey: currKey,
                Revision: 1,
                LevelId: 1,
                Title: "Persistent Curriculum",
                RecommendedGrades: new[] { "ม.1" },
                Subjects: new[] { new CurriculumSubject(Guid.NewGuid(), "SUB-1", "Subject 1") },
                SourceRef: "https://schoolbright.co/src/1",
                Lifecycle: "draft",
                PublishedAt: null,
                IsDelete: false,
                CreateBy: null,
                CreateDate: now,
                UpdateBy: null,
                UpdateDate: now,
                SchoolId: null
            );

            var audit1 = new SchoolBotAuditEventRecord(
                SchoolBotAuditEventId: 0,
                Actor: "author-01",
                Event: "Curriculum.Created",
                EntityType: "SchoolBotCurriculum",
                EntityId: 0,
                RequestId: Guid.NewGuid(),
                Reason: "Curriculum created",
                IsDelete: false,
                CreateBy: null,
                CreateDate: now,
                UpdateBy: null,
                UpdateDate: now,
                SchoolId: null
            );

            var created = await adapter1.CreateCurriculumAsync(record, audit1);
            var etag = OpaqueConcurrencyToken.ToETag(created.UpdateDate);

            var pubAudit = new SchoolBotAuditEventRecord(
                SchoolBotAuditEventId: 0,
                Actor: "approver-02",
                Event: "Curriculum.Published",
                EntityType: "SchoolBotCurriculum",
                EntityId: created.SchoolBotCurriculumId,
                RequestId: Guid.NewGuid(),
                Reason: "APPROVAL-PERSISTENCE-TEST",
                IsDelete: false,
                CreateBy: null,
                CreateDate: now,
                UpdateBy: null,
                UpdateDate: now,
                SchoolId: null
            );

            var published = await adapter1.PublishCurriculumAsync(created.SchoolBotCurriculumId, "APPROVAL-PERSISTENCE-TEST", etag, pubAudit);
            Assert.Equal("published", published.Lifecycle);

            // New process / adapter restart from file store
            var backingStore2 = new FileSchoolBotBackingStore(tempDir);
            var adapter2 = new SchoolBotUnitOfWork(backingStore2);

            var reloaded = await adapter2.GetCurriculumByIdAsync(created.SchoolBotCurriculumId);
            Assert.NotNull(reloaded);
            Assert.Equal("published", reloaded.Lifecycle);
            Assert.Equal("Persistent Curriculum", reloaded.Title);

            var audits = await adapter2.ListAuditEventsByEntityAsync("SchoolBotCurriculum", created.SchoolBotCurriculumId);
            Assert.Equal(2, audits.Count);
            Assert.Contains(audits, a => a.Event == "Curriculum.Published" && a.Reason == "APPROVAL-PERSISTENCE-TEST");
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }
}
