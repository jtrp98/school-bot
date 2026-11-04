using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.ApiV2LicenseDeviceTests;

namespace ServiceSchoolBot.Tests;

internal sealed class LicenseStoreSelfCheckFixture : IDisposable
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
    private readonly string? directory;
    private readonly ISchoolBotBackingStore store;
    public ISchoolBotBackingStore BackingStore => store;
    public string? DirectoryPath => directory;
    public FakeClock Clock { get; } = new(DateTimeOffset.Parse("2026-11-03T00:00:00Z"));
    public TestLicenseTokenSigner Signer { get; } = new();
    public SchoolBotUnitOfWork AdapterA { get; }
    public SchoolBotUnitOfWork AdapterB { get; }
    public ISchoolBotRepositoryScope ProxyA { get; }
    public ISchoolBotRepositoryScope ProxyB { get; }
    public LicenseCommitPause PauseA => (LicenseCommitPause)(object)ProxyA;
    public LicenseCommitPause PauseB => (LicenseCommitPause)(object)ProxyB;
    public LicenseDeviceService DeviceA { get; }
    public LicenseDeviceService DeviceB { get; }
    public LicenseAdminService AdminA { get; }
    public LicenseAdminService AdminB { get; }
    public LicenseAdminAuthContext Admin { get; } = LicenseAdminPrincipalStub.CreateLicenseAdmin(
        principal: "P1", validFrom: DateTimeOffset.Parse("2026-01-01T00:00:00Z"));

    public LicenseStoreSelfCheckFixture(bool file)
    {
        directory = file ? Path.Combine(AppContext.BaseDirectory, "SchoolBotLicenseSelfCheck_" + Guid.NewGuid().ToString("N")) : null;
        store = file ? new FileSchoolBotBackingStore(directory!) : new MemorySchoolBotBackingStore();
        AdapterA = new SchoolBotUnitOfWork(store, Clock);
        AdapterB = new SchoolBotUnitOfWork(NewStore(), Clock);
        ProxyA = DispatchProxy.Create<ISchoolBotRepositoryScope, LicenseCommitPause>();
        ProxyB = DispatchProxy.Create<ISchoolBotRepositoryScope, LicenseCommitPause>();
        PauseA.Inner = AdapterA;
        PauseB.Inner = AdapterB;
        DeviceA = new LicenseDeviceService(ProxyA, Signer, Clock);
        DeviceB = new LicenseDeviceService(ProxyB, Signer, Clock);
        AdminA = new LicenseAdminService(ProxyA, Clock);
        AdminB = new LicenseAdminService(ProxyB, Clock);
    }

    private ISchoolBotBackingStore NewStore() => directory is null ? store : new FileSchoolBotBackingStore(directory);
    public SchoolBotUnitOfWork Reader() => new(NewStore(), Clock);
    public Task<LicenseStoreSnapshot> Snapshot() => NewStore().LoadLicenseUnitAsync();

    public async Task<(string Key, SchoolBotLicenseRecord License)> Create()
    {
        var created = await AdminA.CreateLicenseAsync(Admin, new CreateLicenseV2Request(
            100, "Test School", Clock.GetUtcNow().AddDays(1)));
        await AdapterA.ListLicensesAsync();
        await AdapterB.ListLicensesAsync();
        var license = await Reader().GetLicenseByIdAsync(created.License.Id);
        return (created.Key, license!);
    }

    public async Task<EntitlementResult> Request(
        LicenseDeviceService service, string key, ECDsa device, DeviceJwk jwk, int? bindingId = null)
    {
        var challenge = await service.CreateChallengeAsync(new CreateChallengeRequest(jwk));
        var path = bindingId.HasValue ? "/v2/licenses/validate" : "/v2/licenses/activate";
        object unsigned = bindingId.HasValue
            ? new { key, binding_id = bindingId.Value, challenge = challenge.Challenge }
            : new { key, device_jwk = jwk, challenge = challenge.Challenge };
        var proof = CreateProof(device, challenge.Challenge, "POST", path, null, JsonSerializer.Serialize(unsigned, Options));
        object body = bindingId.HasValue
            ? new { key, binding_id = bindingId.Value, challenge = challenge.Challenge, proof }
            : new { key, device_jwk = jwk, challenge = challenge.Challenge, proof };
        var json = JsonSerializer.Serialize(body, Options);
        return bindingId.HasValue
            ? await service.ValidateAsync(new ValidateLicenseRequest(key, bindingId.Value, challenge.Challenge, proof), json)
            : await service.ActivateAsync(new ActivateLicenseRequest(key, jwk, challenge.Challenge, proof), json);
    }

    public async Task AssertLease(EntitlementResult response)
    {
        var reader = Reader();
        var license = await reader.GetLicenseByIdAsync(response.LicenseId);
        var binding = await reader.GetBindingByIdAsync(response.BindingId);
        Assert.Equal(binding!.LastCheckedAt, response.CheckedAt);
        Assert.Equal(binding.LastEntitlementExpiresAt, response.ExpiresAt);
        Assert.True(Signer.Verify(response.Entitlement, out var header, out var payload));
        using var ownedHeader = header;
        using var ownedPayload = payload;
        var claims = payload!.RootElement;
        Assert.Equal(binding.Epoch, claims.GetProperty("binding_epoch").GetInt32());
        Assert.Equal(binding.DeviceJkt, claims.GetProperty("cnf").GetProperty("jkt").GetString());
        Assert.Equal(binding.SchoolBotLicenseBindingId, claims.GetProperty("binding_id").GetInt32());
        Assert.Equal(response.CheckedAt.ToUnixTimeSeconds(), claims.GetProperty("checked_at").GetInt64());
        Assert.Equal(response.ExpiresAt.ToUnixTimeSeconds(), claims.GetProperty("exp").GetInt64());
        Assert.True(response.ExpiresAt <= license!.ExpiresAt);
    }

    public static async Task<string> Outcome(Task<EntitlementResult> request)
    {
        try { await request; return "200"; }
        catch (DomainRuleException error) { return error.Code; }
    }

    public static void EqualState(LicenseStoreSnapshot expected, LicenseStoreSnapshot actual)
    {
        Assert.Equal(expected.Licenses.OrderBy(l => l.SchoolBotLicenseId), actual.Licenses.OrderBy(l => l.SchoolBotLicenseId));
        Assert.Equal(expected.Bindings.OrderBy(b => b.SchoolBotLicenseBindingId), actual.Bindings.OrderBy(b => b.SchoolBotLicenseBindingId));
    }

    public void Dispose()
    {
        Signer.Dispose();
        if (directory is null) return;
        var resolved = Path.GetFullPath(directory);
        if (!resolved.StartsWith(Path.GetFullPath(AppContext.BaseDirectory), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Unexpected self-check directory.");
        if (Directory.Exists(resolved))
        {
            if ((File.GetAttributes(resolved) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidOperationException("Self-check directory is a link.");
            Directory.Delete(resolved, recursive: true);
        }
    }
}

public class LicenseCommitPause : DispatchProxy
{
    public ISchoolBotRepositoryScope Inner { get; set; } = null!;
    private bool armed;
    public TaskCompletionSource Ready { get; private set; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void Arm()
    {
        armed = true;
        Ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public void Release() => release.TrySetResult();

    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        if (armed && method!.Name == nameof(ISchoolBotRepositoryScope.UpdateBindingAsync))
            return Pause<SchoolBotLicenseBindingRecord>(method, args);
        if (armed && method!.Name == nameof(ISchoolBotRepositoryScope.CommitLicenseActivationAsync))
            return Pause<LicenseActivationCommitResult>(method, args);
        return InvokeInner(method!, args);
    }

    private async Task<T> Pause<T>(MethodInfo method, object?[]? args)
    {
        armed = false;
        Ready.TrySetResult();
        await release.Task.WaitAsync(TimeSpan.FromSeconds(15));
        return await (Task<T>)InvokeInner(method, args)!;
    }

    private object? InvokeInner(MethodInfo method, object?[]? args)
    {
        try { return method.Invoke(Inner, args); }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error.InnerException).Throw();
            throw;
        }
    }
}
