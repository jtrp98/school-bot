using ServiceSchoolBot.Application.Common.Model.Projects;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.Tests;

public sealed class IndependentProjectGateTests
{
    private static Task<GrpcOutcome<Rpc.ProjectDocumentResponse>> Create(
        Rpc.ProjectService.ProjectServiceClient client, string token, Guid key, string name) =>
        GrpcTest.Try(client.CreateProjectAsync(new Rpc.CreateProjectRequest
        {
            IdempotencyKey = key.ToString("D"),
            Mode = "code",
            Name = name,
            ContentJson = JsonSerializer.Serialize(new { kind = "code", source = name })
        }, GrpcTest.As(token)));

    [Fact]
    public async Task SameRequestKeyAcrossOwners_DoesNotReturnAnotherOwnersProject()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Projects(factory);
        var key = Guid.NewGuid();
        var first = await Create(client, ProjectPrincipalStub.StudentToken, key, "private-first");
        Assert.True(first.Succeeded);
        var firstId = first.Response!.Document.ProjectId;
        var second = await Create(client, ProjectPrincipalStub.Student2Token, key, "private-second");
        if (second.Succeeded)
        {
            var body = second.Response!.Document;
            Assert.NotEqual(firstId, body.ProjectId);
            using var content = JsonDocument.Parse(body.ContentJson);
            Assert.Equal("private-second", content.RootElement.GetProperty("source").GetString());
            var store = factory.Services.GetRequiredService<ISchoolBotBackingStore>();
            Assert.Equal(2, (await store.LoadProjectsAsync()).Count);
        }
        else
        {
            Assert.Equal(409, second.HttpStatus);
            Assert.DoesNotContain("private-first", second.Error!.ToString());
        }
    }

    [Fact]
    public async Task SameRequestKeyWithChangedContent_DoesNotReportOldContentAsSuccessfulSave()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Projects(factory);
        var key = Guid.NewGuid();
        Assert.True((await Create(client, ProjectPrincipalStub.StudentToken, key, "original")).Succeeded);
        var changed = await Create(client, ProjectPrincipalStub.StudentToken, key, "changed");
        Assert.Equal(409, changed.HttpStatus);
        var store = factory.Services.GetRequiredService<ISchoolBotBackingStore>();
        Assert.Single(await store.LoadProjectsAsync());
        Assert.Single(await store.LoadProjectRevisionsAsync());
    }
}
