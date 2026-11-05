using Rpc = SchoolBot.Infrastructure.Protos;
using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Common.Model.Projects;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.ApiHelpers;
using static ServiceSchoolBot.Tests.ApiV2LicenseDeviceTests;

namespace ServiceSchoolBot.Tests;

public sealed class IndependentRecheckTests
{
    private static Task<GrpcOutcome<Rpc.ProjectDocumentResponse>> Create(
        Rpc.ProjectService.ProjectServiceClient client, Guid key) =>
        GrpcTest.Try(client.CreateProjectAsync(new Rpc.CreateProjectRequest
        {
            IdempotencyKey = key.ToString("D"),
            Mode = "code",
            Name = "source",
            ContentJson = "{\"kind\":\"code\",\"source\":\"private-source\"}"
        }, GrpcTest.As(ProjectPrincipalStub.StudentToken)));

    private static Task<GrpcOutcome<Rpc.ProjectDocumentResponse>> Copy(
        Rpc.ProjectService.ProjectServiceClient client, int id, int revision, string etag, Guid key,
        string name = "copy", string token = ProjectPrincipalStub.StudentToken) =>
        GrpcTest.Try(client.CopyProjectAsync(new Rpc.CopyProjectRequest
        {
            Id = id,
            SourceProjectRevisionId = revision,
            IfMatch = etag,
            IdempotencyKey = key.ToString("D"),
            Name = name
        }, GrpcTest.As(token)));

    [Theory]
    [InlineData("same")]
    [InlineData("owner")]
    [InlineData("payload")]
    [InlineData("operation")]
    public async Task CopyReplay_RespectsOwnerPayloadAndOperationWithoutDuplicateWrites(string variation)
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Projects(factory);
        var created = await Create(client, Guid.NewGuid());
        Assert.True(created.Succeeded);
        var source = created.Response!.Document;
        var key = Guid.NewGuid();
        var copied = await Copy(client, source.ProjectId, source.ProjectRevisionId, created.Response.Etag, key);
        Assert.True(copied.Succeeded);
        var replay = variation == "operation"
            ? await Create(client, key)
            : await Copy(client, source.ProjectId, source.ProjectRevisionId, created.Response.Etag, key,
                variation == "payload" ? "changed" : "copy",
                variation == "owner" ? ProjectPrincipalStub.Student2Token : ProjectPrincipalStub.StudentToken);
        if (variation == "same")
        {
            Assert.True(replay.Succeeded);
            Assert.Equal(copied.Response, replay.Response);
        }
        else
        {
            Assert.Equal(409, replay.HttpStatus);
            Assert.DoesNotContain("private-source", replay.Error!.ToString());
        }
        var store = factory.Services.GetRequiredService<ISchoolBotBackingStore>();
        Assert.Equal(2, (await store.LoadProjectsAsync()).Count);
        Assert.Equal(2, (await store.LoadProjectRevisionsAsync()).Count);
    }

    private static Task<GrpcOutcome<Rpc.EntitlementResponse>> DeviceRequest(
        Rpc.LicenseService.LicenseServiceClient client, string key, ECDsa device, DeviceJwk jwk, int? binding = null) =>
        IndependentLicenseGateTests.DeviceRequest(client, key, device, jwk, binding);

    [Fact]
    public async Task ValidationOverlappingSuccessfulUnbind_DoesNotRestoreDetachedBinding()
    {
        using var seed = new ApiFactory();
        _ = seed.Services;
        var proxy = DispatchProxy.Create<ISchoolBotRepositoryScope, BindingWritePause>();
        var pause = (BindingWritePause)(object)proxy;
        pause.Inner = seed.Services.GetRequiredService<ISchoolBotRepositoryScope>();
        using var host = seed.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSchoolBotRepositories(proxy)));
        var admin = GrpcTest.Licenses(host);
        var client = GrpcTest.Licenses(host);
        var (key, id) = await CreateV2License(admin, seed);
        var (device, jwk) = CreateDeviceKey();
        using var ownedDevice = device;
        Assert.Equal(200, (await DeviceRequest(client, key, device, jwk)).HttpStatus);
        var store = seed.Services.GetRequiredService<ISchoolBotBackingStore>();
        var before = Assert.Single(await store.LoadBindingsAsync());
        var license = Assert.Single(await store.LoadLicensesAsync());
        pause.Armed = true;
        var validating = DeviceRequest(client, key, device, jwk, before.SchoolBotLicenseBindingId);
        await pause.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try
        {
            await admin.UnbindLicenseAsync(new Rpc.UnbindLicenseRequest
            {
                Id = id,
                IfMatch = ServiceSchoolBot.Domain.Concurrency.OpaqueConcurrencyToken.ToETag(license.UpdateDate),
                Reason = "ย้ายเครื่องขณะตรวจออนไลน์"
            }, AdminV2());
            Assert.NotNull(Assert.Single(await store.LoadBindingsAsync()).DetachedAt);
        }
        finally
        {
            pause.Release.TrySetResult();
        }
        await validating;
        var fresh = new SchoolBotUnitOfWork(store, seed.Clock);
        var afterLicense = await fresh.GetLicenseByIdAsync(id);
        var afterBinding = Assert.Single(await fresh.ListBindingsByLicenseIdAsync(id));
        var reactivated = await DeviceRequest(client, key, device, jwk);
        var activeCount = (await store.LoadBindingsAsync()).Count(b => b.LicenseId == id && b.DetachedAt is null);
        Assert.True(afterLicense!.CurrentBindingId is null && afterBinding.DetachedAt is not null && reactivated.HttpStatus == 409 && activeCount == 0,
            $"หลัง validate ที่ค้างก่อน unbind: CurrentBindingId={afterLicense.CurrentBindingId}, DetachedAt={afterBinding.DetachedAt}, activate เครื่องเก่า={reactivated.HttpStatus}, bindings ที่ไม่ detached={activeCount}; ต้องคงประวัติการถอดเครื่องและห้ามเครื่องเก่ากลับเอง");
    }
}

public class BindingWritePause : DispatchProxy
{
    public ISchoolBotRepositoryScope Inner { get; set; } = null!;
    public bool Armed { get; set; }
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (Armed && targetMethod!.Name == nameof(ISchoolBotRepositoryScope.UpdateBindingAsync))
        {
            Armed = false;
            return Resume(targetMethod, args);
        }
        return targetMethod!.Invoke(Inner, args);
    }

    private async Task<SchoolBotLicenseBindingRecord> Resume(MethodInfo method, object?[]? args)
    {
        Entered.TrySetResult();
        await Release.Task.WaitAsync(TimeSpan.FromSeconds(10));
        return await (Task<SchoolBotLicenseBindingRecord>)method.Invoke(Inner, args)!;
    }
}

