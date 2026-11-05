using Rpc = SchoolBot.Infrastructure.Protos;
using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.ApiHelpers;
using static ServiceSchoolBot.Tests.ApiV2LicenseDeviceTests;

namespace ServiceSchoolBot.Tests;

public sealed class IndependentLicenseGateTests
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    internal static async Task<GrpcOutcome<Rpc.EntitlementResponse>> DeviceRequest(
        Rpc.LicenseService.LicenseServiceClient client, string key, ECDsa device, DeviceJwk jwk, int? binding = null)
    {
        var challenge = await RequestChallenge(client, jwk);
        var path = binding.HasValue ? "/api/service-school-bot/licenses/validate" : "/api/service-school-bot/licenses/activate";
        object unsigned = binding.HasValue
            ? new { key, binding_id = binding.Value, challenge }
            : new { key, device_jwk = jwk, challenge };
        var proof = CreateProof(device, challenge, "POST", path, null, JsonSerializer.Serialize(unsigned, Options));
        object body = binding.HasValue
            ? new { key, binding_id = binding.Value, challenge, proof }
            : new { key, device_jwk = jwk, challenge, proof };
        var rawBody = JsonSerializer.Serialize(body, Options);
        return await GrpcTest.Try(binding.HasValue ? Validate(client, rawBody) : Activate(client, rawBody));
    }

    [Fact]
    public async Task ValidateBeforeActivation_DoesNotWriteAnyBinding()
    {
        using var factory = new ApiFactory();
        var admin = GrpcTest.Licenses(factory);
        var client = GrpcTest.Licenses(factory);
        var (key, id) = await CreateV2License(admin, factory);
        var (device, jwk) = CreateDeviceKey();
        using var ownedDevice = device;
        GrpcTest.AssertError((await DeviceRequest(client, key, device, jwk, 999)).Error!, 403, ErrorCodes.DeviceNotActivated);
        var store = factory.Services.GetRequiredService<ISchoolBotBackingStore>();
        Assert.Empty(await store.LoadBindingsAsync());
        Assert.Null((await store.LoadLicensesAsync()).Single(l => l.SchoolBotLicenseId == id).CurrentBindingId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InactiveLicense_RejectsBothOperationsWithoutRenewingStoredLease(bool revoke)
    {
        using var factory = new ApiFactory();
        var admin = GrpcTest.Licenses(factory);
        var client = GrpcTest.Licenses(factory);
        var (key, _) = await CreateV2License(admin, factory, days: 1);
        var (device, jwk) = CreateDeviceKey();
        using var ownedDevice = device;
        var activated = await DeviceRequest(client, key, device, jwk);
        Assert.Equal(200, activated.HttpStatus);
        var binding = activated.Response!.BindingId;
        var store = factory.Services.GetRequiredService<ISchoolBotBackingStore>();
        var before = Assert.Single(await store.LoadBindingsAsync());
        if (revoke)
        {
            var response = await admin.RevokeLicenseAsync(new Rpc.RevokeLicenseRequest
            { Id = before.LicenseId, Reason = "test revoke" }, AdminV2());
            Assert.True(response.License.Revoked);
        }
        else
        {
            factory.Clock.Advance(TimeSpan.FromDays(1));
        }
        GrpcTest.AssertError((await DeviceRequest(client, key, device, jwk)).Error!, 403, ErrorCodes.LicenseInactive);
        GrpcTest.AssertError((await DeviceRequest(client, key, device, jwk, binding)).Error!, 403, ErrorCodes.LicenseInactive);
        Assert.Equal(before, Assert.Single(await store.LoadBindingsAsync()));
    }

    [Fact]
    public async Task SignedLease_HasVerifiedSignatureAndExpectedDeviceThumbprint()
    {
        using var factory = new ApiFactory();
        var admin = GrpcTest.Licenses(factory);
        var client = GrpcTest.Licenses(factory);
        var (key, id) = await CreateV2License(admin, factory);
        var (device, jwk) = CreateDeviceKey();
        using var ownedDevice = device;
        var response = await DeviceRequest(client, key, device, jwk);
        Assert.Equal(200, response.HttpStatus);
        var body = response.Response!;
        var signer = factory.Services.GetRequiredService<ILicenseTokenSigner>();
        Assert.True(signer.Verify(body.Entitlement, out var header, out var payload));
        using var ownedHeader = header;
        using var ownedPayload = payload;
        var canonicalJwk = $"{{\"crv\":\"P-256\",\"kty\":\"EC\",\"x\":\"{jwk.X}\",\"y\":\"{jwk.Y}\"}}";
        var thumbprint = Convert.ToHexStringLower(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(canonicalJwk)));
        var claims = payload!.RootElement;
        Assert.Equal("schoolbot-entitlement+jwt", header!.RootElement.GetProperty("typ").GetString());
        Assert.Equal(thumbprint, claims.GetProperty("cnf").GetProperty("jkt").GetString());
        Assert.Equal(id, claims.GetProperty("license_id").GetInt32());
        Assert.Equal(body.BindingId, claims.GetProperty("binding_id").GetInt32());
        Assert.Equal(claims.GetProperty("checked_at").GetInt64() + 259200, claims.GetProperty("exp").GetInt64());
    }

    [Fact]
    public async Task FailedUnbindWithStaleEtag_DoesNotDetachCurrentBindingInStore()
    {
        using var factory = new ApiFactory();
        var admin = GrpcTest.Licenses(factory);
        var client = GrpcTest.Licenses(factory);
        var (key, id) = await CreateV2License(admin, factory);
        var (device, jwk) = CreateDeviceKey();
        using var ownedDevice = device;
        Assert.Equal(200, (await DeviceRequest(client, key, device, jwk)).HttpStatus);
        var store = factory.Services.GetRequiredService<ISchoolBotBackingStore>();
        var before = Assert.Single(await store.LoadBindingsAsync());
        await GrpcTest.AssertError(admin.UnbindLicenseAsync(new Rpc.UnbindLicenseRequest
        {
            Id = id,
            IfMatch = "\"stale\"",
            Reason = "test stale screen"
        }, AdminV2()), 412, ErrorCodes.VersionConflict);
        var after = Assert.Single(await store.LoadBindingsAsync());
        var validation = await DeviceRequest(client, key, device, jwk, before.SchoolBotLicenseBindingId);
        Assert.True(before == after && validation.HttpStatus == 200,
            $"หลัง unbind ถูกปฏิเสธ: DetachedAt={after.DetachedAt}, validate={validation.HttpStatus}; คาดว่า state เดิมและ validate=OK");
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public async Task LicenseExpiryBoundary_AppliesToActivateAndValidate(int secondsFromExpiry)
    {
        using var factory = new ApiFactory();
        var admin = GrpcTest.Licenses(factory);
        var client = GrpcTest.Licenses(factory);
        var (key, _) = await CreateV2License(admin, factory, days: 1);
        var (device, jwk) = CreateDeviceKey();
        using var ownedDevice = device;
        var first = await DeviceRequest(client, key, device, jwk);
        Assert.Equal(200, first.HttpStatus);
        var binding = first.Response!.BindingId;
        factory.Clock.Advance(TimeSpan.FromDays(1) + TimeSpan.FromSeconds(secondsFromExpiry));
        var activated = await DeviceRequest(client, key, device, jwk);
        var validated = await DeviceRequest(client, key, device, jwk, binding);
        if (secondsFromExpiry < 0)
        {
            Assert.Equal(200, activated.HttpStatus);
            Assert.Equal(200, validated.HttpStatus);
        }
        else
        {
            GrpcTest.AssertError(activated.Error!, 403, ErrorCodes.LicenseInactive);
            GrpcTest.AssertError(validated.Error!, 403, ErrorCodes.LicenseInactive);
        }
        var store = factory.Services.GetRequiredService<ISchoolBotBackingStore>();
        Assert.Single(await store.LoadBindingsAsync());
    }

    [Fact]
    public async Task SuccessfulUnbind_PersistsDetachedHistoryAndClearsCurrentBinding()
    {
        using var factory = new ApiFactory();
        var admin = GrpcTest.Licenses(factory);
        var client = GrpcTest.Licenses(factory);
        var (key, id) = await CreateV2License(admin, factory);
        var (device, jwk) = CreateDeviceKey();
        using var ownedDevice = device;
        Assert.Equal(200, (await DeviceRequest(client, key, device, jwk)).HttpStatus);
        var store = factory.Services.GetRequiredService<ISchoolBotBackingStore>();
        var license = (await store.LoadLicensesAsync()).Single();
        factory.Clock.Advance(TimeSpan.FromSeconds(1));
        await admin.UnbindLicenseAsync(new Rpc.UnbindLicenseRequest
        {
            Id = id,
            IfMatch = ServiceSchoolBot.Domain.Concurrency.OpaqueConcurrencyToken.ToETag(license.UpdateDate),
            Reason = "test move"
        }, AdminV2());
        var fresh = new SchoolBotUnitOfWork(store, factory.Clock);
        var reloaded = await fresh.GetLicenseByIdAsync(id);
        Assert.Null(reloaded!.CurrentBindingId);
        Assert.Equal(license.BindingEpoch + 1, reloaded.BindingEpoch);
        Assert.Equal(factory.Clock.GetUtcNow(), Assert.Single(await fresh.ListBindingsByLicenseIdAsync(id)).DetachedAt);
    }
}

