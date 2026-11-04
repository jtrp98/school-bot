using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Domain;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.Tests;

public sealed class ApiV2LicenseDeviceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    internal static (ECDsa Key, DeviceJwk Jwk) CreateDeviceKey()
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var p = ecdsa.ExportParameters(false);
        var jwk = new DeviceJwk(
            Kty: "EC",
            Crv: "P-256",
            X: TestLicenseTokenSigner.Base64UrlEncode(p.Q.X!),
            Y: TestLicenseTokenSigner.Base64UrlEncode(p.Q.Y!)
        );
        return (ecdsa, jwk);
    }

    internal static string CreateProof(
        ECDsa deviceKey,
        string challenge,
        string method,
        string path,
        string? idempotencyKey,
        string bodyWithoutProofJson)
    {
        using var doc = JsonDocument.Parse(bodyWithoutProofJson);
        var canonicalJson = DeviceProofVerifier.Canonicalize(doc.RootElement);
        var bodyHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalJson)));
        var header = new { typ = "JWT", alg = "ES256" };
        var headerJson = JsonSerializer.Serialize(header);
        var headerB64 = TestLicenseTokenSigner.Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));

        var payload = new
        {
            challenge,
            method,
            path,
            idempotency_key = idempotencyKey,
            body_hash = bodyHash
        };
        var payloadJson = JsonSerializer.Serialize(payload, JsonOptions);
        var payloadB64 = TestLicenseTokenSigner.Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

        var data = Encoding.UTF8.GetBytes($"{headerB64}.{payloadB64}");
        var sig = deviceKey.SignData(data, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        var sigB64 = TestLicenseTokenSigner.Base64UrlEncode(sig);

        return $"{headerB64}.{payloadB64}.{sigB64}";
    }

    internal static async Task<(string Key, int LicenseId)> CreateV2License(
        Rpc.LicenseService.LicenseServiceClient client, ApiFactory factory, int schoolId = 100, int days = 30)
    {
        var created = await client.CreateLicenseAsync(new Rpc.CreateLicenseRequest
        {
            SchoolId = schoolId,
            SchoolName = "Test School Academy",
            ExpiresAt = factory.Clock.GetUtcNow().AddDays(days).ToString("O")
        }, ApiHelpers.AdminV2());
        return (created.Key, created.License.Id);
    }

    internal static async Task<string> RequestChallenge(Rpc.LicenseService.LicenseServiceClient client, DeviceJwk jwk) =>
        (await client.CreateDeviceChallengeAsync(
            Body(JsonSerializer.Serialize(new { device_jwk = jwk }, JsonOptions)), GrpcTest.Headers())).Challenge;

    internal static Rpc.DeviceBodyRequest Body(string rawBody, string? idempotencyKey = null)
    {
        var request = new Rpc.DeviceBodyRequest { RawBody = rawBody };
        if (idempotencyKey is not null) request.IdempotencyKey = idempotencyKey;
        return request;
    }

    internal static AsyncUnaryCall<Rpc.EntitlementResponse> Activate(
        Rpc.LicenseService.LicenseServiceClient client, string rawBody, string? idempotencyKey = null) =>
        client.ActivateLicenseAsync(Body(rawBody, idempotencyKey), GrpcTest.Headers());

    internal static AsyncUnaryCall<Rpc.EntitlementResponse> Validate(
        Rpc.LicenseService.LicenseServiceClient client, string rawBody) =>
        client.ValidateLicenseAsync(Body(rawBody), GrpcTest.Headers());

    private static string ActivateBody(ECDsa deviceKey, DeviceJwk jwk, string challenge, string key)
    {
        var withoutProof = JsonSerializer.Serialize(new { challenge, device_jwk = jwk, key }, JsonOptions);
        var proof = CreateProof(deviceKey, challenge, "POST", "/v2/licenses/activate", null, withoutProof);
        return JsonSerializer.Serialize(new { key, device_jwk = jwk, challenge, proof }, JsonOptions);
    }

    [Fact]
    public async Task ActivateAndValidate_SuccessFlow_IssuesSignedEntitlement()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var (key, licenseId) = await CreateV2License(client, factory);
        var (deviceKey, deviceJwk) = CreateDeviceKey();

        var challenge = await RequestChallenge(client, deviceJwk);

        var activatePayloadWithoutProof = JsonSerializer.Serialize(new
        {
            challenge,
            device_jwk = deviceJwk,
            key
        }, JsonOptions);
        var proof = CreateProof(deviceKey, challenge, "POST", "/v2/licenses/activate", null, activatePayloadWithoutProof);

        var activateBody = JsonSerializer.Serialize(new
        {
            key,
            device_jwk = deviceJwk,
            challenge,
            proof
        }, JsonOptions);

        var activateJson = await Activate(client, activateBody);

        Assert.True(activateJson.Valid);
        Assert.Equal(licenseId, activateJson.LicenseId);
        var bindingId = activateJson.BindingId;
        Assert.True(bindingId > 0);

        var entitlementToken = activateJson.Entitlement;
        Assert.False(string.IsNullOrWhiteSpace(entitlementToken));

        var parts = entitlementToken.Split('.');
        Assert.Equal(3, parts.Length);
        var claimsJson = Encoding.UTF8.GetString(TestLicenseTokenSigner.Base64UrlDecode(parts[1]));
        var claims = JsonNode.Parse(claimsJson)!;

        Assert.Equal("schoolbot-api", claims["iss"]!.GetValue<string>());
        Assert.Equal("schoolbot-desktop", claims["aud"]!.GetValue<string>());
        Assert.Equal(licenseId, claims["license_id"]!.GetValue<int>());
        Assert.Equal(bindingId, claims["binding_id"]!.GetValue<int>());
        Assert.Equal(0, claims["binding_epoch"]!.GetValue<int>());
        Assert.NotNull(claims["cnf"]!["jkt"]);
        Assert.Equal(3, claims["modes"]!.AsArray().Count);

        var challengeVal = await RequestChallenge(client, deviceJwk);
        var validatePayloadWithoutProof = JsonSerializer.Serialize(new
        {
            binding_id = bindingId,
            challenge = challengeVal,
            key
        }, JsonOptions);
        var proofVal = CreateProof(deviceKey, challengeVal, "POST", "/v2/licenses/validate", null, validatePayloadWithoutProof);

        var validateBody = JsonSerializer.Serialize(new
        {
            key,
            binding_id = bindingId,
            challenge = challengeVal,
            proof = proofVal
        }, JsonOptions);

        var validateJson = await Validate(client, validateBody);

        Assert.True(validateJson.Valid);
        Assert.Equal(bindingId, validateJson.BindingId);

        using var scope = factory.Services.CreateScope();
        var dbAdapter = scope.ServiceProvider.GetRequiredService<ISchoolBotRepositoryScope>();
        var bindings = await dbAdapter.ListBindingsByLicenseIdAsync(licenseId);
        Assert.Single(bindings);
        var lic = await dbAdapter.GetLicenseByIdAsync(licenseId);
        Assert.Equal(0, lic!.BindingEpoch);
    }

    [Fact]
    public async Task SameDevice_Reactivate_RenewsWithoutAddingBindingOrEpoch()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var (key, licenseId) = await CreateV2License(client, factory);
        var (deviceKey, deviceJwk) = CreateDeviceKey();

        var challenge1 = await RequestChallenge(client, deviceJwk);
        var json1 = await Activate(client, ActivateBody(deviceKey, deviceJwk, challenge1, key));
        var bindingId1 = json1.BindingId;

        factory.Clock.Advance(TimeSpan.FromHours(1));

        var challenge2 = await RequestChallenge(client, deviceJwk);
        var json2 = await Activate(client, ActivateBody(deviceKey, deviceJwk, challenge2, key));
        var bindingId2 = json2.BindingId;

        Assert.Equal(bindingId1, bindingId2);

        using var scope = factory.Services.CreateScope();
        var dbAdapter = scope.ServiceProvider.GetRequiredService<ISchoolBotRepositoryScope>();
        var bindings = await dbAdapter.ListBindingsByLicenseIdAsync(licenseId);
        Assert.Single(bindings);
        var lic = await dbAdapter.GetLicenseByIdAsync(licenseId);
        Assert.Equal(0, lic!.BindingEpoch);
    }

    [Fact]
    public async Task SecondDevice_Activate_RejectedWithDeviceLimit()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var (key, licenseId) = await CreateV2License(client, factory);
        var (devAKey, devAJwk) = CreateDeviceKey();
        var (devBKey, devBJwk) = CreateDeviceKey();

        var chalA = await RequestChallenge(client, devAJwk);
        await Activate(client, ActivateBody(devAKey, devAJwk, chalA, key));

        var chalB = await RequestChallenge(client, devBJwk);
        var errorB = await GrpcTest.Fails(Activate(client, ActivateBody(devBKey, devBJwk, chalB, key)));

        Assert.Equal(409, errorB.HttpStatus);
        Assert.Equal(ErrorCodes.DeviceLimit, errorB.Code);

        using var scope = factory.Services.CreateScope();
        var dbAdapter = scope.ServiceProvider.GetRequiredService<ISchoolBotRepositoryScope>();
        var bindings = await dbAdapter.ListBindingsByLicenseIdAsync(licenseId);
        Assert.Single(bindings);
    }

    [Fact]
    public async Task ConcurrentActivate_RaceResultsInSingleBinding()
    {
        using var factory = new ApiFactory();
        var admin = GrpcTest.Licenses(factory);

        var (key, licenseId) = await CreateV2License(admin, factory);
        var (devAKey, devAJwk) = CreateDeviceKey();
        var (devBKey, devBJwk) = CreateDeviceKey();

        var clientA = GrpcTest.Licenses(factory);
        var clientB = GrpcTest.Licenses(factory);

        var chalA = await RequestChallenge(clientA, devAJwk);
        var chalB = await RequestChallenge(clientB, devBJwk);

        var bodyA = ActivateBody(devAKey, devAJwk, chalA, key);
        var bodyB = ActivateBody(devBKey, devBJwk, chalB, key);

        var taskA = GrpcTest.Try(Activate(clientA, bodyA));
        var taskB = GrpcTest.Try(Activate(clientB, bodyB));

        var statusA = (await taskA).HttpStatus;
        var statusB = (await taskB).HttpStatus;

        Assert.True(
            (statusA == 200 && statusB == 409) ||
            (statusB == 200 && statusA == 409),
            $"Expected one OK and one Conflict, got A={statusA}, B={statusB}");

        using var scope = factory.Services.CreateScope();
        var dbAdapter = scope.ServiceProvider.GetRequiredService<ISchoolBotRepositoryScope>();
        var bindings = await dbAdapter.ListBindingsByLicenseIdAsync(licenseId);
        Assert.Single(bindings);
    }
}
