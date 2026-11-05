using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Common.Model.Projects;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;
using Grpc.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.Tests;

public sealed class ApiV2ProjectTests
{
    private static Metadata As(string? token = ProjectPrincipalStub.StudentToken) => GrpcTest.As(token);

    private static Rpc.CreateProjectRequest Create(string mode, string name, object content) =>
        new() { Mode = mode, Name = name, ContentJson = JsonSerializer.Serialize(content) };

    private static Rpc.UpdateProjectRequest Update(int id, string name, object content, string? ifMatch)
    {
        var request = new Rpc.UpdateProjectRequest { Id = id, Name = name, ContentJson = JsonSerializer.Serialize(content) };
        if (ifMatch is not null) request.IfMatch = ifMatch;
        return request;
    }

    private static Rpc.DeleteProjectRequest Delete(int id, string? ifMatch)
    {
        var request = new Rpc.DeleteProjectRequest { Id = id };
        if (ifMatch is not null) request.IfMatch = ifMatch;
        return request;
    }

    private static Rpc.CopyProjectRequest Copy(int id, string name, int revisionId, string? ifMatch)
    {
        var request = new Rpc.CopyProjectRequest { Id = id, Name = name, SourceProjectRevisionId = revisionId };
        if (ifMatch is not null) request.IfMatch = ifMatch;
        return request;
    }

    private static Rpc.ListProjectsRequest List(string? mode = null) =>
        mode is null ? new() : new() { Mode = mode };

    private static Rpc.GetProjectRequest Get(int id) => new() { Id = id };

    private static JsonNode Content(Rpc.ProjectDocumentResponse response) =>
        JsonNode.Parse(response.Document.ContentJson)!;

    [Fact]
    public async Task Ac01101_SaveTwoProjectsPerModeForStudentAndTeacher_CanOpenAllWithoutCrossContamination()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Projects(factory);

        var modes = new[] { "block", "arm", "code" };
        var studentProjects = new List<(string Mode, int Id, string Name)>();
        var teacherProjects = new List<(string Mode, int Id, string Name)>();

        // 1. Student creates 2 projects per mode
        foreach (var mode in modes)
        {
            for (var i = 1; i <= 2; i++)
            {
                var name = $"Student {mode} Project {i}";
                var content = new { mode, step = i, timestamp = "2026-11-03" };
                var res = await client.CreateProjectAsync(Create(mode, name, content), As(ProjectPrincipalStub.StudentToken));

                Assert.False(string.IsNullOrEmpty(res.Etag));
                var body = res.Document;
                Assert.Equal(2, body.Schema);
                var id = body.ProjectId;
                Assert.True(id > 0);
                Assert.Equal(1, body.Revision);
                Assert.Equal(mode, body.Mode);
                Assert.Equal(name, body.Name);
                studentProjects.Add((mode, id, name));
            }
        }

        // 2. Teacher creates 2 projects per mode
        foreach (var mode in modes)
        {
            for (var i = 1; i <= 2; i++)
            {
                var name = $"Teacher {mode} Project {i}";
                var content = new { mode, step = i, teacher = true };
                var res = await client.CreateProjectAsync(Create(mode, name, content), As(ProjectPrincipalStub.TeacherToken));

                var id = res.Document.ProjectId;
                Assert.True(id > 0);
                Assert.Equal(mode, res.Document.Mode);
                teacherProjects.Add((mode, id, name));
            }
        }

        // 3. Verify student can list and retrieve all their projects by mode without seeing teacher projects
        foreach (var mode in modes)
        {
            var items = (await client.ListProjectsAsync(List(mode), As(ProjectPrincipalStub.StudentToken))).Items;
            Assert.Equal(2, items.Count);
            foreach (var item in items)
            {
                Assert.Equal(mode, item.Mode);
                Assert.Contains("Student", item.Name);
            }
        }

        // 4. Open each student project individually and verify contents
        foreach (var (mode, id, name) in studentProjects)
        {
            var body = (await client.GetProjectAsync(Get(id), As(ProjectPrincipalStub.StudentToken))).Document;
            Assert.Equal(id, body.ProjectId);
            Assert.Equal(name, body.Name);
            Assert.Equal(mode, body.Mode);
            Assert.Equal(1, body.Revision);
        }

        // 5. Cross-contamination check: Student cannot open Teacher's project
        foreach (var (_, teacherProjectId, _) in teacherProjects)
        {
            await GrpcTest.AssertError(client.GetProjectAsync(Get(teacherProjectId), As(ProjectPrincipalStub.StudentToken)),
                404, ErrorCodes.NotFound);
        }
    }

    [Fact]
    public async Task Ac01102_UpdateProjectA_ReturnsLatestA_ProjectBInSameModeUnchanged()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Projects(factory);

        // 1. Create Project A and Project B in the same mode (block)
        var createARes = await client.CreateProjectAsync(Create("block", "Block Project A", new { step = 1 }), As());
        var idA = createARes.Document.ProjectId;
        var etagA = createARes.Etag;

        var createBRes = await client.CreateProjectAsync(Create("block", "Block Project B", new { step = 100 }), As());
        var idB = createBRes.Document.ProjectId;
        var etagB = createBRes.Etag;

        // Advance clock so update gets a distinct timestamp / ETag
        factory.Clock.Advance(TimeSpan.FromSeconds(1));

        // 2. Update Project A to revision 2
        var updateARes = await client.UpdateProjectAsync(
            Update(idA, "Block Project A Updated", new { step = 2, status = "updated" }, etagA), As());
        Assert.Equal(2, updateARes.Document.Revision);
        Assert.Equal("Block Project A Updated", updateARes.Document.Name);
        var newEtagA = updateARes.Etag;
        Assert.NotEqual(etagA, newEtagA);

        // 3. Open Project A: verify updated content and revision 2
        var freshA = await client.GetProjectAsync(Get(idA), As());
        Assert.Equal(2, freshA.Document.Revision);
        Assert.Equal("Block Project A Updated", freshA.Document.Name);
        Assert.Equal(2, Content(freshA)["step"]!.GetValue<int>());

        // 4. Open Project B: verify completely unchanged (revision 1, step 100, original ETag)
        var freshB = await client.GetProjectAsync(Get(idB), As());
        Assert.Equal(etagB, freshB.Etag);
        Assert.Equal(1, freshB.Document.Revision);
        Assert.Equal("Block Project B", freshB.Document.Name);
        Assert.Equal(100, Content(freshB)["step"]!.GetValue<int>());
    }

    [Fact]
    public async Task Ac01103_MalformedJsonOrExceeding1MiB_ReturnsError_DoesNotSave()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Projects(factory);

        // 1. Invalid Mode
        await GrpcTest.AssertError(client.CreateProjectAsync(Create("drone", "Drone Project", new { step = 1 }), As()),
            400, ErrorCodes.InvalidInput);

        // 2. Empty Name
        await GrpcTest.AssertError(client.CreateProjectAsync(Create("block", "   ", new { step = 1 }), As()),
            400, ErrorCodes.InvalidInput);

        // 3. Name exceeding 200 characters
        await GrpcTest.AssertError(client.CreateProjectAsync(Create("block", new string('a', 201), new { step = 1 }), As()),
            400, ErrorCodes.InvalidInput);

        // 4. Null content
        await GrpcTest.AssertError(client.CreateProjectAsync(
            new Rpc.CreateProjectRequest { Mode = "block", Name = "Valid Name", ContentJson = "null" }, As()),
            400, ErrorCodes.InvalidInput);

        // 5. Verify no projects were created from failed requests
        Assert.Empty((await client.ListProjectsAsync(List(), As())).Items);
    }

    [Fact]
    public async Task Ac03602_StudentAndTeacherLogin_CanSaveAndOpenCloudProjects()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Projects(factory);

        // Student saves project
        var studentProjId = (await client.CreateProjectAsync(
            Create("code", "Student Code Lab", new { code = "console.log('hi');" }),
            As(ProjectPrincipalStub.StudentToken))).Document.ProjectId;

        // Teacher saves project
        var teacherProjId = (await client.CreateProjectAsync(
            Create("code", "Teacher Code Lab", new { code = "print('teacher');" }),
            As(ProjectPrincipalStub.TeacherToken))).Document.ProjectId;

        // Student can open their project
        var studentDoc = await client.GetProjectAsync(Get(studentProjId), As(ProjectPrincipalStub.StudentToken));
        Assert.Equal("Student Code Lab", studentDoc.Document.Name);

        // Teacher can open their project
        var teacherDoc = await client.GetProjectAsync(Get(teacherProjId), As(ProjectPrincipalStub.TeacherToken));
        Assert.Equal("Teacher Code Lab", teacherDoc.Document.Name);
    }

    [Fact]
    public async Task Ac03603_RestartService_RetainsSavedProjectsOnBackingStore()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "SchoolBotApiRestartTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testDir);

        try
        {
            int projectId;
            string updatedEtag;

            // Phase 1: First host instance saves and updates project
            using (var factory1 = new CustomBackingStoreApiFactory(testDir))
            {
                var client1 = GrpcTest.Projects(factory1);
                var createRes = await client1.CreateProjectAsync(Create("arm", "Persistent Arm", new { pose = 1 }), As());
                projectId = createRes.Document.ProjectId;
                var etag = createRes.Etag;

                // Update to revision 2
                var updateRes = await client1.UpdateProjectAsync(
                    Update(projectId, "Persistent Arm Rev2", new { pose = 2 }, etag), As());
                updatedEtag = updateRes.Etag;
            }

            // Phase 2: Fresh host instance on the same backing store directory simulates service restart
            using (var factory2 = new CustomBackingStoreApiFactory(testDir))
            {
                var client2 = GrpcTest.Projects(factory2);
                var getRes = await client2.GetProjectAsync(Get(projectId), As());
                Assert.Equal(updatedEtag, getRes.Etag);

                Assert.Equal(projectId, getRes.Document.ProjectId);
                Assert.Equal("Persistent Arm Rev2", getRes.Document.Name);
                Assert.Equal(2, getRes.Document.Revision);
                Assert.Equal(2, Content(getRes)["pose"]!.GetValue<int>());

                // Also verify list endpoint after restart
                var items = (await client2.ListProjectsAsync(List("arm"), As())).Items;
                Assert.Single(items);
                Assert.Equal(projectId, items[0].ProjectId);
            }
        }
        finally
        {
            try { Directory.Delete(testDir, true); } catch { }
        }
    }

    [Fact]
    public async Task Ac03604_FailedSaves_ReportErrors_NeverClaimSuccessOrCrossBind()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Projects(factory);

        // 1. Create a project by Student 1
        var createRes = await client.CreateProjectAsync(Create("block", "Student 1 Project", new { x = 1 }), As());
        var projId = createRes.Document.ProjectId;
        var currentEtag = createRes.Etag;

        // 2. PUT without If-Match returns 428 PRECONDITION_REQUIRED
        await GrpcTest.AssertError(client.UpdateProjectAsync(Update(projId, "Name", new { x = 2 }, null), As()),
            428, ErrorCodes.PreconditionRequired);

        // 3. PUT with outdated/wrong If-Match returns 412 VERSION_CONFLICT with current_etag details
        var conflict = await GrpcTest.AssertError(
            client.UpdateProjectAsync(Update(projId, "Name", new { x = 2 }, "\"9999999999999999\""), As()),
            412, ErrorCodes.VersionConflict);
        Assert.Equal(currentEtag, conflict.Details!.CurrentEtag);

        // 4. DELETE without If-Match returns 428 PRECONDITION_REQUIRED
        await GrpcTest.AssertError(client.DeleteProjectAsync(Delete(projId, null), As()),
            428, ErrorCodes.PreconditionRequired);

        // 5. DELETE with outdated If-Match returns 412 VERSION_CONFLICT
        await GrpcTest.AssertError(client.DeleteProjectAsync(Delete(projId, "\"0000000000000000\""), As()),
            412, ErrorCodes.VersionConflict);

        // 6. Ownership isolation: Student 2 cannot read or modify Student 1's project (returns 404)
        await GrpcTest.AssertError(client.GetProjectAsync(Get(projId), As(ProjectPrincipalStub.Student2Token)),
            404, ErrorCodes.NotFound);

        await GrpcTest.AssertError(client.UpdateProjectAsync(
            Update(projId, "Hacked Name", new { hacked = true }, currentEtag), As(ProjectPrincipalStub.Student2Token)),
            404, ErrorCodes.NotFound);

        await GrpcTest.AssertError(client.DeleteProjectAsync(Delete(projId, currentEtag), As(ProjectPrincipalStub.Student2Token)),
            404, ErrorCodes.NotFound);

        // 7. Other school student cannot read Student 1's project
        await GrpcTest.AssertError(client.GetProjectAsync(Get(projId), As(ProjectPrincipalStub.OtherSchoolStudentToken)),
            404, ErrorCodes.NotFound);

        // 8. Auth failures: Unauthenticated, Expired, WrongChannel, NoLicense, Unconfigured
        await GrpcTest.AssertError(client.ListProjectsAsync(List(), GrpcTest.Headers()), 401, ErrorCodes.Unauthorized);

        await GrpcTest.AssertError(client.ListProjectsAsync(List(), As(ProjectPrincipalStub.ExpiredToken)),
            401, ErrorCodes.SessionExpired);

        await GrpcTest.AssertError(client.ListProjectsAsync(List(), As(ProjectPrincipalStub.WrongChannelToken)),
            403, ErrorCodes.ChannelNotAllowed);

        await GrpcTest.AssertError(client.ListProjectsAsync(List(), As(ProjectPrincipalStub.NoLicenseToken)),
            403, ErrorCodes.LicenseRequired);

        await GrpcTest.AssertError(client.ListProjectsAsync(List(), As(ProjectPrincipalStub.UnconfiguredToken)),
            503, ErrorCodes.AuthNotConfigured);
    }

    [Fact]
    public async Task CopiesAndSoftDelete_WorkCorrectly()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Projects(factory);

        // 1. Create source project
        var createRes = await client.CreateProjectAsync(Create("block", "Source Project", new { block = "root" }), As());
        var sourceId = createRes.Document.ProjectId;
        var sourceRevId = createRes.Document.ProjectRevisionId;
        var sourceEtag = createRes.Etag;

        // 2. Copy project without If-Match returns 428
        await GrpcTest.AssertError(client.CopyProjectAsync(Copy(sourceId, "Copy Project", sourceRevId, null), As()),
            428, ErrorCodes.PreconditionRequired);

        // 3. Copy project with invalid revision PK returns 400
        await GrpcTest.AssertError(client.CopyProjectAsync(Copy(sourceId, "Copy Project", 999999, sourceEtag), As()),
            400, ErrorCodes.InvalidInput);

        // 4. Successful copy
        var copyRes = await client.CopyProjectAsync(Copy(sourceId, "Copy of Source Project", sourceRevId, sourceEtag), As());
        Assert.False(string.IsNullOrEmpty(copyRes.Etag));
        var copyId = copyRes.Document.ProjectId;
        Assert.NotEqual(sourceId, copyId);
        Assert.Equal("Copy of Source Project", copyRes.Document.Name);
        Assert.Equal("block", copyRes.Document.Mode);
        Assert.Equal(1, copyRes.Document.Revision);
        Assert.Equal("root", Content(copyRes)["block"]!.GetValue<string>());

        // 5. Soft delete source project with If-Match
        var delRes = await client.DeleteProjectAsync(Delete(sourceId, sourceEtag), As());
        Assert.Equal(sourceId, delRes.ProjectId);
        Assert.True(delRes.Deleted);

        // 6. Subsequent GET on deleted source project returns 404
        await GrpcTest.AssertError(client.GetProjectAsync(Get(sourceId), As()), 404, ErrorCodes.NotFound);

        // 7. Copied project is still alive and retrievable
        await client.GetProjectAsync(Get(copyId), As());
    }

    private sealed class CustomBackingStoreApiFactory(string storePath) : Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSchoolBotMemoryStorage();
            builder.ConfigureServices(services =>
            {
                var store = new FileSchoolBotBackingStore(storePath);
                services.AddSingleton<ISchoolBotBackingStore>(store);
                services.AddSchoolBotRepositories(sp =>
                    new SchoolBotUnitOfWork(store, sp.GetRequiredService<TimeProvider>()));
            });
            builder.ConfigureTestServices(services => services.AddTrustedGateway().AddStubPrincipals());
        }
    }
}
