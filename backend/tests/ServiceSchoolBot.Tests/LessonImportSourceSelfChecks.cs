using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using System.Text.Json;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Curriculum;
using Xunit;

namespace ServiceSchoolBot.Tests;

public sealed class LessonImportSourceSelfChecks
{
    [Fact]
    public void NullImportSourcePreservesSerializedBytes()
    {
        var lesson = new SchoolBotLessonRecord(1, 2, Guid.Empty, 1, "title", "objective", "instructions",
            false, null, null, null, null);
        var expected = "{\"SchoolBotLessonId\":1,\"CurriculumId\":2,\"LessonKey\":\"00000000-0000-0000-0000-000000000000\",\"Ordinal\":1,\"Title\":\"title\",\"Objective\":\"objective\",\"Instructions\":\"instructions\",\"IsDelete\":false,\"CreateBy\":null,\"CreateDate\":null,\"UpdateBy\":null,\"UpdateDate\":null,\"SchoolId\":null}";
        Assert.Equal(expected, JsonSerializer.Serialize(lesson));
        var snapshot = new ServiceSchoolBot.Infrastructure.Repositories.Storage.CurriculumStoreSnapshot(
            1, [], [lesson], [], [], [], []);
        var expectedSnapshot = "{\"format\":1,\"curricula\":[],\"lessons\":[" + expected +
            "],\"missions\":[],\"criteria\":[],\"activities\":[],\"audit_events\":[],\"idempotency\":null}";
        Assert.Equal(System.Text.Encoding.UTF8.GetBytes(expectedSnapshot), JsonSerializer.SerializeToUtf8Bytes(snapshot));
    }

    [Fact]
    public void CodecPreservesNullApprovalAndRejectsMismatchedKey()
    {
        var key = Guid.NewGuid();
        var source = new LessonImportSource("fixture", key.ToString("D"), null, new string('a', 64));
        var json = LessonImportSourceCodec.Encode(source, key);
        Assert.Equal(source, LessonImportSourceCodec.Decode(json, key));
        Assert.Throws<JsonException>(() => LessonImportSourceCodec.Decode(json, Guid.NewGuid()));
    }

    [Fact]
    public async Task ContentPutCopiesImportSourceAndConflictLeavesStateUnchanged()
    {
        using var fixture = new CurriculumImportFixture(false);
        await fixture.Initialize();
        var client = GrpcTest.Curricula(fixture);
        var request = await fixture.Normalize("base");
        var imported = await CurriculumImportFixture.Import(client, request, Guid.NewGuid());
        var afterImport = await fixture.Snapshot();

        var conflictRows = request.Rows.Select((row, index) => index == 1 ? row with { SourceApproved = true } : row).ToArray();
        var error = await GrpcTest.Fails(CurriculumImportFixture.Import(client, request with { Rows = conflictRows }, Guid.NewGuid()));
        Assert.Equal(409, error.HttpStatus);
        Assert.Equal(ErrorCodes.StateConflict, error.Code);
        Assert.Equal(JsonSerializer.Serialize(afterImport), JsonSerializer.Serialize(await fixture.Snapshot()));

        await client.UpdateCurriculumContentAsync(CurriculumRpc.Content(1, new UpdateCurriculumContentRequest(
                request.Rows.Select(row => row.Lesson with { Title = "{\"th\":\"แก้ไขแล้ว\"}" }).ToArray()),
            imported.Etag, Guid.NewGuid()), GrpcTest.Headers());
        var afterPut = await fixture.Snapshot();
        Assert.Equal(afterImport.Lessons.Select(row => row.ImportSource), afterPut.Lessons.Select(row => row.ImportSource));
    }
}
