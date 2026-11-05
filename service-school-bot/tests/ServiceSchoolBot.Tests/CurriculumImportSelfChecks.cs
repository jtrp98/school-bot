using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using System.Security.Cryptography;
using Google.Protobuf;
using System.Text.Json;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

public sealed class CurriculumImportSelfChecks
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreatesDraftWithProvenanceAndAllMedia(bool file)
    {
        using var fixture = new CurriculumImportFixture(file);
        await fixture.Initialize();
        var client = GrpcTest.Curricula(fixture);
        var request = await fixture.Normalize("base");
        var response = await CurriculumImportFixture.Import(client, request, Guid.NewGuid());
        var body = CurriculumRpc.ToModel(response);
        Assert.All(body.Lessons, lesson => Assert.Equal("created", lesson.Outcome));
        Assert.Equal(3, body.Lessons.Select(lesson => lesson.LessonId).Distinct().Count());
        var state = await fixture.Snapshot();
        AssertState(state, 1, 3, 1, 1);
        Assert.Equal(fixture.Clock.GetUtcNow(), state.Curricula[0].UpdateDate);
        var audit = Assert.Single(state.AuditEvents);
        Assert.Equal("Curriculum.LessonsImported", audit.Event);
        Assert.Equal("Curriculum lessons imported", audit.Reason);
        Assert.Equal("SchoolBotCurriculum", audit.EntityType);
        Assert.Equal(1, audit.EntityId);
        var ledger = Assert.Single(state.Idempotency!);
        Assert.Equal("import_curriculum_lessons", ledger.Operation);
        Assert.Equal("staff_admin:1001", ledger.Principal);
        Assert.Null(ledger.SchoolId);
        Assert.Equal(1, ledger.ResourceId);
        Assert.Equal(200, ledger.HttpStatus);
        Assert.NotEmpty(ledger.ResultCipher!);
        Assert.Equal(body, JsonSerializer.Deserialize<CurriculumImportResponse>(JsonSerializer.Serialize(body))! with
        {
            Lessons = body.Lessons
        });
        var read = await client.GetAdminCurriculumAsync(new() { Id = 1 }, GrpcTest.Headers());
        var view = CurriculumRpc.ToModel(read);
        Assert.Equal("draft", view.Lifecycle);
        Assert.Null(view.PublishedAt);
        Assert.Null(view.Approval);
        Assert.Equal(response.Etag, read.Etag);
        Assert.Equal(1, view.Revision);
        Assert.Equal(1, view.LevelId);
        Assert.Equal(new bool?[] { true, false, null }, view.Lessons.Select(lesson => lesson.ImportSource!.SourceApproved));
        for (var index = 0; index < 3; index++)
        {
            var lesson = view.Lessons[index];
            var source = lesson.ImportSource!;
            Assert.Equal(CurriculumImportFixture.Key(index + 3), lesson.LessonKey);
            Assert.Equal(lesson.LessonKey.ToString("D"), source.SourceRowKey);
            Assert.Equal("fixture:course-plan-096", source.SourceRef);
            Assert.Equal(ExpectedHash(request, request.Rows[index]), source.RowHash);
            Assert.Equal(source, LessonImportSourceCodec.Decode(state.Lessons[index].ImportSource!, lesson.LessonKey));
            using var config = JsonDocument.Parse(Assert.Single(lesson.Activities).Config!);
            Assert.Equal(index == 0 ? 5 : 0, config.RootElement.GetProperty("media_refs").GetArrayLength());
        }
        Assert.Equal(JsonSerializer.Serialize(state), JsonSerializer.Serialize(await fixture.Snapshot()));
    }

    [Theory]
    [InlineData(false, "o")]
    [InlineData(true, "o")]
    [InlineData(false, "reorder")]
    [InlineData(true, "reorder")]
    public async Task ChangedColumnOAndReorderingPreserveImportedRows(bool file, string variant)
    {
        using var fixture = new CurriculumImportFixture(file);
        await fixture.Initialize();
        var client = GrpcTest.Curricula(fixture);
        var baseline = await fixture.Normalize("base");
        var first = await CurriculumImportFixture.Import(client, baseline, Guid.NewGuid());
        var before = await fixture.Snapshot();
        fixture.Clock.Now = fixture.Clock.Now.AddSeconds(10);
        var request = await fixture.Normalize(variant);
        if (variant == "o") Assert.Equal(JsonSerializer.Serialize(baseline), JsonSerializer.Serialize(request));
        var response = await CurriculumImportFixture.Import(client, request, Guid.NewGuid());
        var body = CurriculumRpc.ToModel(response);
        Assert.Equal(request.Rows.Select(row => row.SourceRowKey), body.Lessons.Select(row => row.SourceRowKey));
        Assert.All(body.Lessons, lesson =>
        {
            Assert.Equal("unchanged", lesson.Outcome);
            Assert.Equal(before.Lessons.Single(row => row.LessonKey == lesson.SourceRowKey).SchoolBotLessonId, lesson.LessonId);
        });
        Assert.Equal(first.Etag, response.Etag);
        var after = await fixture.Snapshot();
        AssertState(after, 1, 3, 1, 2);
        Assert.Equal(JsonSerializer.Serialize(before with { Idempotency = null }),
            JsonSerializer.Serialize(after with { Idempotency = null }));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReopenedFileAdapterReplaysOrRecognizesUnchangedRows(bool sameKey)
    {
        using var fixture = new CurriculumImportFixture(true);
        await fixture.Initialize();
        var client = GrpcTest.Curricula(fixture);
        var request = await fixture.Normalize("base");
        var key = Guid.NewGuid();
        var first = await CurriculumImportFixture.Import(client, request, key);
        var before = await fixture.Snapshot();
        fixture.Reopen();
        fixture.Clock.Now = fixture.Clock.Now.AddSeconds(10);
        var response = await CurriculumImportFixture.Import(client, request, sameKey ? key : Guid.NewGuid());
        Assert.Equal(first.Etag, response.Etag);
        if (sameKey) Assert.Equal(first.ToByteArray(), response.ToByteArray());
        else Assert.All(response.Lessons, lesson => Assert.Equal("unchanged", lesson.Outcome));
        var after = await fixture.Snapshot();
        AssertState(after, 1, 3, 1, sameKey ? 1 : 2);
        Assert.Equal(JsonSerializer.Serialize(before with { Idempotency = null }),
            JsonSerializer.Serialize(after with { Idempotency = null }));
        if (sameKey) Assert.Equal(JsonSerializer.Serialize(before), JsonSerializer.Serialize(after));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NewRevisionKeepsLessonKeysAndCreatesDistinctRows(bool file)
    {
        using var fixture = new CurriculumImportFixture(file);
        await fixture.Initialize();
        var client = GrpcTest.Curricula(fixture);
        var request = await fixture.Normalize("base");
        await CurriculumImportFixture.Import(client, request, Guid.NewGuid());
        var original = await fixture.Snapshot();
        var seed = fixture.Seed;
        var created = await client.CreateCurriculumAsync(CurriculumRpc.Create(new CreateCurriculumRequest(
            seed.CurriculumKey, 2, seed.LevelId, seed.Title, seed.SourceRef, seed.RecommendedGrades, seed.Subjects),
            Guid.NewGuid()), GrpcTest.Headers());
        var revision = CurriculumRpc.ToModel(created);
        var baseline = await fixture.Snapshot();
        AssertState(baseline, 2, 3, 2, 2);
        var response = await CurriculumImportFixture.Import(client, request, Guid.NewGuid(), revision.Id);
        Assert.All(response.Lessons, lesson => Assert.Equal("created", lesson.Outcome));
        var after = await fixture.Snapshot();
        AssertState(after, 2, 6, 3, 3);
        var reader = fixture.Reader();
        var oldLessons = await reader.ListLessonsByCurriculumIdAsync(1);
        var newLessons = await reader.ListLessonsByCurriculumIdAsync(revision.Id);
        Assert.Equal(oldLessons.Select(lesson => lesson.LessonKey), newLessons.Select(lesson => lesson.LessonKey));
        Assert.Empty(oldLessons.Select(lesson => lesson.SchoolBotLessonId)
            .Intersect(newLessons.Select(lesson => lesson.SchoolBotLessonId)));
        Assert.Equal(JsonSerializer.Serialize(original.Lessons), JsonSerializer.Serialize(oldLessons));
        Assert.Equal(JsonSerializer.Serialize(original.Curricula[0]),
            JsonSerializer.Serialize(after.Curricula.Single(row => row.Revision == 1)));
        Assert.All(after.Curricula, row => Assert.Equal(1, row.LevelId));
    }

    private static string ExpectedHash(CurriculumImportRequest request, CurriculumImportRow row)
    {
        var payload = new
        {
            source_ref = request.SourceRef,
            source_row_key = row.SourceRowKey.ToString("D"),
            source_approved = row.SourceApproved,
            title = row.Lesson.Title,
            objective = row.Lesson.Objective,
            instructions = row.Lesson.Instructions,
            missions = row.Lesson.Missions,
            activities = row.Lesson.Activities
        };
        return Convert.ToHexStringLower(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(payload)));
    }

    private static void AssertState(CurriculumStoreSnapshot state, int curricula, int lessons, int audits, int ledger)
    {
        Assert.Equal(curricula, state.Curricula.Count);
        Assert.Equal(lessons, state.Lessons.Count);
        Assert.Equal(lessons, state.Activities.Count);
        Assert.Empty(state.Missions);
        Assert.Empty(state.Criteria);
        Assert.Equal(audits, state.AuditEvents.Count);
        Assert.Equal(ledger, state.Idempotency!.Count);
        Assert.All(state.Lessons, lesson => Assert.Null(lesson.SchoolId));
        Assert.All(state.Activities, activity => Assert.Null(activity.SchoolId));
        Assert.All(state.Curricula, row => Assert.Equal("draft", row.Lifecycle));
    }
}
