using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.ApiV2LicenseDeviceTests;

namespace ServiceSchoolBot.Tests;

public sealed class IndependentLicenseRaceTests
{
    [Fact]
    public async Task ConcurrentHosts_ActivatingDifferentDevices_CommitOnlyOneCurrentBinding()
    {
        using var seed = new ApiFactory();
        var shared = seed.Services.GetRequiredService<ISchoolBotRepositoryScope>();
        var gate = DispatchProxy.Create<ISchoolBotRepositoryScope, LicenseLookupBarrier>();
        ((LicenseLookupBarrier)(object)gate).Inner = shared;
        using var hostA = seed.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSchoolBotRepositories(gate)));
        using var hostB = seed.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSchoolBotRepositories(gate)));
        var admin = GrpcTest.Licenses(hostA);
        var a = GrpcTest.Licenses(hostA);
        var b = GrpcTest.Licenses(hostB);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        for (var round = 0; round < 50; round++)
        {
            var (key, id) = await CreateV2License(admin, seed);
            var (keyA, jwkA) = CreateDeviceKey();
            var (keyB, jwkB) = CreateDeviceKey();
            using var ownedA = keyA;
            using var ownedB = keyB;
            var challengeA = await RequestChallenge(a, jwkA);
            var challengeB = await RequestChallenge(b, jwkB);
            var path = "/v2/licenses/activate";
            var proofA = CreateProof(keyA, challengeA, "POST", path, null,
                JsonSerializer.Serialize(new { key, device_jwk = jwkA, challenge = challengeA }, options));
            var proofB = CreateProof(keyB, challengeB, "POST", path, null,
                JsonSerializer.Serialize(new { key, device_jwk = jwkB, challenge = challengeB }, options));
            ((LicenseLookupBarrier)(object)gate).Arm();
            var responses = await Task.WhenAll(
                GrpcTest.Try(Activate(a, JsonSerializer.Serialize(new { key, device_jwk = jwkA, challenge = challengeA, proof = proofA }, options))),
                GrpcTest.Try(Activate(b, JsonSerializer.Serialize(new { key, device_jwk = jwkB, challenge = challengeB, proof = proofB }, options))));
            var storedBindings = (await seed.Services.GetRequiredService<ISchoolBotBackingStore>().LoadBindingsAsync())
                .Where(binding => binding.LicenseId == id && binding.DetachedAt is null).ToArray();
            Assert.True(responses.Count(r => r.HttpStatus == 200) == 1 &&
                responses.Count(r => r.HttpStatus == 409) == 1 && storedBindings.Length == 1,
                $"รอบ {round + 1}: statuses={string.Join(",", responses.Select(r => r.HttpStatus))}, bindings ที่ไม่ detached={storedBindings.Length}; คาดว่า OK/Conflict และ binding เดียว");
        }
    }
}

public class LicenseLookupBarrier : DispatchProxy
{
    public ISchoolBotRepositoryScope Inner { get; set; } = null!;
    private TaskCompletionSource ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int arrivals;
    private bool armed;

    public void Arm()
    {
        ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        arrivals = 0;
        armed = true;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        try
        {
            var result = targetMethod!.Invoke(Inner, args);
            return armed && targetMethod.Name == nameof(ISchoolBotRepositoryScope.GetLicenseByKeyHashAsync)
                ? WaitForBoth((Task<SchoolBotLicenseRecord?>)result!)
                : result;
        }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        {
            // Reflection must preserve the repository exception seen by the normal error mapper.
            ExceptionDispatchInfo.Capture(error.InnerException).Throw();
            throw;
        }
    }

    private async Task<SchoolBotLicenseRecord?> WaitForBoth(Task<SchoolBotLicenseRecord?> read)
    {
        var snapshot = await read;
        if (Interlocked.Increment(ref arrivals) == 2)
        {
            armed = false;
            ready.TrySetResult();
        }
        await ready.Task.WaitAsync(TimeSpan.FromSeconds(10));
        return snapshot;
    }
}
