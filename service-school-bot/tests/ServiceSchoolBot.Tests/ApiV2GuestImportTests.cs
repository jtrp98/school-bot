using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Application.Common.Model.Projects;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.Tests;

public sealed class ApiV2GuestImportTests
{
    private static string SampleCodeJson(string source = "move_right(3)\njump()") =>
        JsonSerializer.Serialize(new { kind = "code", source });

    private static string SampleArmJson(bool includeConnected = false)
    {
        var p1 = Guid.NewGuid().ToString("D");
        var p2 = Guid.NewGuid().ToString("D");
        var g1 = Guid.NewGuid().ToString("D");
        var dict = new Dictionary<string, object>
        {
            ["kind"] = "arm",
            ["poses"] = new[]
            {
                new
                {
                    id = p1,
                    name = "Home",
                    values = new[] { 0.0, 10.0, -10.0, 0.0, 0.0, 50.0 },
                    duration_ms = 1500,
                    joint_duration_ms = new[] { 1500, 1500, 1500, 1500, 1500, 1500 }
                },
                new
                {
                    id = p2,
                    name = "Pick",
                    values = new[] { 30.0, 20.0, -20.0, 5.0, 0.0, 80.0 },
                    duration_ms = 2000,
                    joint_duration_ms = new[] { 2000, 2000, 2000, 2000, 2000, 2000 }
                }
            },
            ["order"] = new[] { p1, p2 },
            ["groups"] = new[]
            {
                new
                {
                    id = g1,
                    name = "Group 1",
                    pose_ids = new[] { p1, p2 }
                }
            },
            ["joint_names"] = new[] { "J1", "J2", "J3", "J4", "J5", "Gripper" },
            ["time_mode"] = "same",
            ["loops"] = 1,
            ["view"] = "cards"
        };
        if (includeConnected)
            dict["connected"] = true;
        return JsonSerializer.Serialize(dict);
    }

    [Fact]
    public async Task ImportProject_FirstCallAndReplay_CreatesSingleProjectAndAuditEvent_MatchesAc04101And04102()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Projects(factory);
        var localId = Guid.NewGuid().ToString("D");

        var req = new Rpc.ImportProjectRequest
        {
            IdempotencyKey = localId,
            LocalProjectId = localId,
            Mode = "code",
            Name = "Guest Imported Adventure",
            LocalRevision = 3,
            ContentJson = SampleCodeJson("move_right(4)")
        };

        var first = await client.ImportProjectAsync(req, GrpcTest.As(ProjectPrincipalStub.StudentToken));
        Assert.False(first.Replayed);
        Assert.Equal(2, first.Document.Schema);
        Assert.True(first.Document.ProjectId > 0);
        Assert.True(first.Document.ProjectRevisionId > 0);
        Assert.Equal(1, first.Document.Revision);
        Assert.Equal("Guest Imported Adventure", first.Document.Name);
        Assert.False(string.IsNullOrWhiteSpace(first.Etag));

        // Verify audit event recorded with provenance guest-project:<local_project_id>
        var repo = factory.Services.GetRequiredService<ISchoolBotRepositoryScope>();
        var audits = await repo.ListAuditEventsByEntityAsync("SchoolBotProject", first.Document.ProjectId);
        Assert.Single(audits);
        Assert.Equal("GuestProject.Import", audits[0].Event);
        Assert.Equal($"guest-project:{localId}", audits[0].Reason);

        // Retry with identical payload replays the exact cloud project without creating a duplicate
        var second = await client.ImportProjectAsync(req, GrpcTest.As(ProjectPrincipalStub.StudentToken));
        Assert.True(second.Replayed);
        Assert.Equal(first.Document.ProjectId, second.Document.ProjectId);
        Assert.Equal(first.Document.ProjectRevisionId, second.Document.ProjectRevisionId);

        var listed = await client.ListProjectsAsync(new Rpc.ListProjectsRequest(),
            GrpcTest.As(ProjectPrincipalStub.StudentToken));
        Assert.Single(listed.Items);
    }

    [Fact]
    public async Task ImportProject_ModifiedPayloadWithSameLocalProjectId_Returns409Conflict_MatchesAc04103()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Projects(factory);
        var localId = Guid.NewGuid().ToString("D");

        await client.ImportProjectAsync(new Rpc.ImportProjectRequest
        {
            IdempotencyKey = localId,
            LocalProjectId = localId,
            Mode = "code",
            Name = "Initial Guest Project",
            LocalRevision = 1,
            ContentJson = SampleCodeJson("move_right(1)")
        }, GrpcTest.As(ProjectPrincipalStub.StudentToken));

        var conflict = await GrpcTest.AssertError(
            client.ImportProjectAsync(new Rpc.ImportProjectRequest
            {
                IdempotencyKey = localId,
                LocalProjectId = localId,
                Mode = "code",
                Name = "Initial Guest Project",
                LocalRevision = 2,
                ContentJson = SampleCodeJson("move_right(2)")
            }, GrpcTest.As(ProjectPrincipalStub.StudentToken)),
            409,
            ErrorCodes.IdempotencyConflict);
        Assert.NotNull(conflict);

        var listed = await client.ListProjectsAsync(new Rpc.ListProjectsRequest(),
            GrpcTest.As(ProjectPrincipalStub.StudentToken));
        Assert.Single(listed.Items);
    }

    [Fact]
    public async Task ImportProject_ValidationGuards_RejectsMismatchedKeyAndArmRuntimeFields_MatchesAc04104()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Projects(factory);
        var localId = Guid.NewGuid().ToString("D");

        // Mismatched Idempotency-Key != local_project_id -> 400 INVALID_INPUT
        await GrpcTest.AssertError(
            client.ImportProjectAsync(new Rpc.ImportProjectRequest
            {
                IdempotencyKey = Guid.NewGuid().ToString("D"),
                LocalProjectId = localId,
                Mode = "arm",
                Name = "Arm Import",
                LocalRevision = 1,
                ContentJson = SampleArmJson(includeConnected: false)
            }, GrpcTest.As(ProjectPrincipalStub.StudentToken)),
            400,
            ErrorCodes.InvalidInput);

        // Arm content with runtime field "connected": true -> 400 INVALID_INPUT
        await GrpcTest.AssertError(
            client.ImportProjectAsync(new Rpc.ImportProjectRequest
            {
                IdempotencyKey = localId,
                LocalProjectId = localId,
                Mode = "arm",
                Name = "Arm Import With Runtime State",
                LocalRevision = 1,
                ContentJson = SampleArmJson(includeConnected: true)
            }, GrpcTest.As(ProjectPrincipalStub.StudentToken)),
            400,
            ErrorCodes.InvalidInput);

        // Valid Arm content succeeds on the same localId because previous invalid attempts did not write
        var validArm = await client.ImportProjectAsync(new Rpc.ImportProjectRequest
        {
            IdempotencyKey = localId,
            LocalProjectId = localId,
            Mode = "arm",
            Name = "Valid Arm Import",
            LocalRevision = 1,
            ContentJson = SampleArmJson(includeConnected: false)
        }, GrpcTest.As(ProjectPrincipalStub.StudentToken));
        Assert.False(validArm.Replayed);
        Assert.Equal("arm", validArm.Document.Mode);
    }

    [Fact]
    public async Task ImportProject_CrossOwnerAndCrossSchoolIsolation_CannotHijackOrReadAnotherOwnersProject_MatchesAc04105()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Projects(factory);
        var sharedLocalId = Guid.NewGuid().ToString("D");

        var student1Import = await client.ImportProjectAsync(new Rpc.ImportProjectRequest
        {
            IdempotencyKey = sharedLocalId,
            LocalProjectId = sharedLocalId,
            Mode = "code",
            Name = "Student 1 Private Project",
            LocalRevision = 1,
            ContentJson = SampleCodeJson("student1_secret()")
        }, GrpcTest.As(ProjectPrincipalStub.StudentToken));

        var student2Import = await client.ImportProjectAsync(new Rpc.ImportProjectRequest
        {
            IdempotencyKey = sharedLocalId,
            LocalProjectId = sharedLocalId,
            Mode = "code",
            Name = "Student 2 Independent Project",
            LocalRevision = 1,
            ContentJson = SampleCodeJson("student2_own_code()")
        }, GrpcTest.As(ProjectPrincipalStub.Student2Token));

        Assert.False(student2Import.Replayed);
        Assert.NotEqual(student1Import.Document.ProjectId, student2Import.Document.ProjectId);
        Assert.DoesNotContain("student1_secret", student2Import.Document.ContentJson);
        Assert.Contains("student2_own_code", student2Import.Document.ContentJson);

        var otherSchoolImport = await client.ImportProjectAsync(new Rpc.ImportProjectRequest
        {
            IdempotencyKey = sharedLocalId,
            LocalProjectId = sharedLocalId,
            Mode = "code",
            Name = "Other School Project",
            LocalRevision = 1,
            ContentJson = SampleCodeJson("school2_code()")
        }, GrpcTest.As(ProjectPrincipalStub.OtherSchoolStudentToken));

        Assert.False(otherSchoolImport.Replayed);
        Assert.NotEqual(student1Import.Document.ProjectId, otherSchoolImport.Document.ProjectId);
        Assert.NotEqual(student2Import.Document.ProjectId, otherSchoolImport.Document.ProjectId);
    }
}
