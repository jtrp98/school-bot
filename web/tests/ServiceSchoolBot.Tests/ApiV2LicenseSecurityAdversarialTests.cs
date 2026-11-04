using System.Text.Json;
using ServiceSchoolBot.Domain;
using static ServiceSchoolBot.Tests.ApiHelpers;
using static ServiceSchoolBot.Tests.ApiV2LicenseDeviceTests;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.Tests;

public sealed class ApiV2LicenseSecurityAdversarialTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private static async Task AssertCode(Grpc.Core.AsyncUnaryCall<Rpc.EntitlementResponse> call, int httpStatus, string code)
    {
        var error = await GrpcTest.Fails(call);
        Assert.Equal(httpStatus, error.HttpStatus);
        Assert.Equal(code, error.Code);
    }

    [Fact]
    public async Task ReplayChallengeNonce_RejectedWithDeviceProofInvalid()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var (key, _) = await CreateV2License(client, factory);
        var (deviceKey, deviceJwk) = CreateDeviceKey();

        var challenge = await RequestChallenge(client, deviceJwk);

        var p = JsonSerializer.Serialize(new { challenge, device_jwk = deviceJwk, key }, JsonOptions);
        var proof = CreateProof(deviceKey, challenge, "POST", "/v2/licenses/activate", null, p);
        var body = JsonSerializer.Serialize(new { key, device_jwk = deviceJwk, challenge, proof }, JsonOptions);

        await Activate(client, body);

        await AssertCode(Activate(client, body), 403, ErrorCodes.DeviceProofInvalid);
    }

    [Fact]
    public async Task ExpiredChallenge_RejectedWithDeviceProofInvalid()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var (key, _) = await CreateV2License(client, factory);
        var (deviceKey, deviceJwk) = CreateDeviceKey();

        var challenge = await RequestChallenge(client, deviceJwk);

        factory.Clock.Advance(TimeSpan.FromSeconds(121));

        var p = JsonSerializer.Serialize(new { challenge, device_jwk = deviceJwk, key }, JsonOptions);
        var proof = CreateProof(deviceKey, challenge, "POST", "/v2/licenses/activate", null, p);
        var body = JsonSerializer.Serialize(new { key, device_jwk = deviceJwk, challenge, proof }, JsonOptions);

        await AssertCode(Activate(client, body), 403, ErrorCodes.DeviceProofInvalid);
    }

    [Fact]
    public async Task TamperedProofSignature_RejectedWithDeviceProofInvalid()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var (key, _) = await CreateV2License(client, factory);
        var (deviceKey, deviceJwk) = CreateDeviceKey();

        var challenge = await RequestChallenge(client, deviceJwk);
        var p = JsonSerializer.Serialize(new { challenge, device_jwk = deviceJwk, key }, JsonOptions);
        var proof = CreateProof(deviceKey, challenge, "POST", "/v2/licenses/activate", null, p);

        var parts = proof.Split('.');
        var tamperedSig = parts[2][..^2] + (parts[2].EndsWith("a") ? "b" : "a");
        var tamperedProof = $"{parts[0]}.{parts[1]}.{tamperedSig}";

        var body = JsonSerializer.Serialize(new { key, device_jwk = deviceJwk, challenge, proof = tamperedProof }, JsonOptions);

        await AssertCode(Activate(client, body), 403, ErrorCodes.DeviceProofInvalid);
    }

    [Fact]
    public async Task TamperedChallengeSignature_RejectedWithDeviceProofInvalid()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var (key, _) = await CreateV2License(client, factory);
        var (deviceKey, deviceJwk) = CreateDeviceKey();

        var challenge = await RequestChallenge(client, deviceJwk);
        var parts = challenge.Split('.');
        var tamperedChallenge = $"{parts[0]}.{parts[1]}.{parts[2][..^2]}xx";

        var p = JsonSerializer.Serialize(new { challenge = tamperedChallenge, device_jwk = deviceJwk, key }, JsonOptions);
        var proof = CreateProof(deviceKey, tamperedChallenge, "POST", "/v2/licenses/activate", null, p);
        var body = JsonSerializer.Serialize(new { key, device_jwk = deviceJwk, challenge = tamperedChallenge, proof }, JsonOptions);

        await AssertCode(Activate(client, body), 403, ErrorCodes.DeviceProofInvalid);
    }

    [Fact]
    public async Task WrongDeviceProof_DeviceBSignsForDeviceA_Rejected()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var (key, _) = await CreateV2License(client, factory);
        var (devAKey, devAJwk) = CreateDeviceKey();
        var (devBKey, _) = CreateDeviceKey();

        var challenge = await RequestChallenge(client, devAJwk);

        var p = JsonSerializer.Serialize(new { challenge, device_jwk = devAJwk, key }, JsonOptions);
        var proofSignedByB = CreateProof(devBKey, challenge, "POST", "/v2/licenses/activate", null, p);
        var body = JsonSerializer.Serialize(new { key, device_jwk = devAJwk, challenge, proof = proofSignedByB }, JsonOptions);

        await AssertCode(Activate(client, body), 403, ErrorCodes.DeviceProofInvalid);
    }

    [Fact]
    public async Task ChallengeIssuedForDeviceA_SentWithDeviceB_Rejected()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var (key, _) = await CreateV2License(client, factory);
        var (_, devAJwk) = CreateDeviceKey();
        var (devBKey, devBJwk) = CreateDeviceKey();

        var chalA = await RequestChallenge(client, devAJwk);

        var p = JsonSerializer.Serialize(new { challenge = chalA, device_jwk = devBJwk, key }, JsonOptions);
        var proofB = CreateProof(devBKey, chalA, "POST", "/v2/licenses/activate", null, p);
        var body = JsonSerializer.Serialize(new { key, device_jwk = devBJwk, challenge = chalA, proof = proofB }, JsonOptions);

        await AssertCode(Activate(client, body), 403, ErrorCodes.DeviceProofInvalid);
    }

    [Fact]
    public async Task RevokedAndExpiredLicense_RejectedWithLicenseInactive()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var (key, licenseId) = await CreateV2License(client, factory, days: 10);
        var (deviceKey, deviceJwk) = CreateDeviceKey();

        await client.RevokeLicenseAsync(new Rpc.RevokeLicenseRequest { Id = licenseId, Reason = "Test revoke" }, AdminV2());

        var chal = await RequestChallenge(client, deviceJwk);
        var p = JsonSerializer.Serialize(new { challenge = chal, device_jwk = deviceJwk, key }, JsonOptions);
        var proof = CreateProof(deviceKey, chal, "POST", "/v2/licenses/activate", null, p);
        var body = JsonSerializer.Serialize(new { key, device_jwk = deviceJwk, challenge = chal, proof }, JsonOptions);

        await AssertCode(Activate(client, body), 403, ErrorCodes.LicenseInactive);

        var (key2, _) = await CreateV2License(client, factory, days: 5);
        factory.Clock.Advance(TimeSpan.FromDays(6));

        var chal2 = await RequestChallenge(client, deviceJwk);
        var p2 = JsonSerializer.Serialize(new { challenge = chal2, device_jwk = deviceJwk, key = key2 }, JsonOptions);
        var proof2 = CreateProof(deviceKey, chal2, "POST", "/v2/licenses/activate", null, p2);
        var body2 = JsonSerializer.Serialize(new { key = key2, device_jwk = deviceJwk, challenge = chal2, proof = proof2 }, JsonOptions);

        await AssertCode(Activate(client, body2), 403, ErrorCodes.LicenseInactive);
    }

    [Fact]
    public async Task UnbindLifecycle_DetachedDeviceDenied_NewDeviceAllowed()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var (key, licenseId) = await CreateV2License(client, factory);
        var (devAKey, devAJwk) = CreateDeviceKey();
        var (devBKey, devBJwk) = CreateDeviceKey();

        var chalA = await RequestChallenge(client, devAJwk);
        var pA = JsonSerializer.Serialize(new { challenge = chalA, device_jwk = devAJwk, key }, JsonOptions);
        var proofA = CreateProof(devAKey, chalA, "POST", "/v2/licenses/activate", null, pA);
        var bodyA = JsonSerializer.Serialize(new { key, device_jwk = devAJwk, challenge = chalA, proof = proofA }, JsonOptions);
        var jsonA = await Activate(client, bodyA);
        var bindingIdA = jsonA.BindingId;

        await client.UnbindLicenseAsync(new Rpc.UnbindLicenseRequest { Id = licenseId, Reason = "Move machine" }, AdminV2());

        var chalVal = await RequestChallenge(client, devAJwk);
        var pVal = JsonSerializer.Serialize(new { binding_id = bindingIdA, challenge = chalVal, key }, JsonOptions);
        var proofVal = CreateProof(devAKey, chalVal, "POST", "/v2/licenses/validate", null, pVal);
        var bodyVal = JsonSerializer.Serialize(new { key, binding_id = bindingIdA, challenge = chalVal, proof = proofVal }, JsonOptions);
        await AssertCode(Validate(client, bodyVal), 403, ErrorCodes.BindingRevoked);

        var chalReA = await RequestChallenge(client, devAJwk);
        var pReA = JsonSerializer.Serialize(new { challenge = chalReA, device_jwk = devAJwk, key }, JsonOptions);
        var proofReA = CreateProof(devAKey, chalReA, "POST", "/v2/licenses/activate", null, pReA);
        var bodyReA = JsonSerializer.Serialize(new { key, device_jwk = devAJwk, challenge = chalReA, proof = proofReA }, JsonOptions);
        await AssertCode(Activate(client, bodyReA), 409, ErrorCodes.DeviceLimit);

        var chalB = await RequestChallenge(client, devBJwk);
        var pB = JsonSerializer.Serialize(new { challenge = chalB, device_jwk = devBJwk, key }, JsonOptions);
        var proofB = CreateProof(devBKey, chalB, "POST", "/v2/licenses/activate", null, pB);
        var bodyB = JsonSerializer.Serialize(new { key, device_jwk = devBJwk, challenge = chalB, proof = proofB }, JsonOptions);
        var jsonB = await Activate(client, bodyB);
        Assert.True(jsonB.BindingId > bindingIdA);
    }

    [Fact]
    public async Task LeaseExpiry_MinOf3DaysAndLicenseExpiry()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var (keyFar, _) = await CreateV2License(client, factory, days: 30);
        var (devKey1, devJwk1) = CreateDeviceKey();
        var chal1 = await RequestChallenge(client, devJwk1);
        var p1 = JsonSerializer.Serialize(new { challenge = chal1, device_jwk = devJwk1, key = keyFar }, JsonOptions);
        var proof1 = CreateProof(devKey1, chal1, "POST", "/v2/licenses/activate", null, p1);
        var body1 = JsonSerializer.Serialize(new { key = keyFar, device_jwk = devJwk1, challenge = chal1, proof = proof1 }, JsonOptions);
        var json1 = await Activate(client, body1);
        var checkedAt1 = DateTimeOffset.Parse(json1.CheckedAt);
        var expiresAt1 = DateTimeOffset.Parse(json1.ExpiresAt);
        Assert.Equal(TimeSpan.FromDays(3), expiresAt1 - checkedAt1);

        var (keyNear, _) = await CreateV2License(client, factory, days: 1);
        var (devKey2, devJwk2) = CreateDeviceKey();
        var chal2 = await RequestChallenge(client, devJwk2);
        var p2 = JsonSerializer.Serialize(new { challenge = chal2, device_jwk = devJwk2, key = keyNear }, JsonOptions);
        var proof2 = CreateProof(devKey2, chal2, "POST", "/v2/licenses/activate", null, p2);
        var body2 = JsonSerializer.Serialize(new { key = keyNear, device_jwk = devJwk2, challenge = chal2, proof = proof2 }, JsonOptions);
        var json2 = await Activate(client, body2);
        var checkedAt2 = DateTimeOffset.Parse(json2.CheckedAt);
        var expiresAt2 = DateTimeOffset.Parse(json2.ExpiresAt);
        Assert.True(expiresAt2 - checkedAt2 <= TimeSpan.FromDays(1) + TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Idempotency_SamePayloadReplays_DifferentPayloadWithStaleProofRejected()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var (key, _) = await CreateV2License(client, factory);
        var (devKey, devJwk) = CreateDeviceKey();
        var idempKey = Guid.NewGuid().ToString("D");

        var chal = await RequestChallenge(client, devJwk);
        var p = JsonSerializer.Serialize(new { challenge = chal, device_jwk = devJwk, key }, JsonOptions);
        var proof = CreateProof(devKey, chal, "POST", "/v2/licenses/activate", idempKey, p);
        var body = JsonSerializer.Serialize(new { key, device_jwk = devJwk, challenge = chal, proof }, JsonOptions);

        var json1 = await Activate(client, body, idempKey);

        var json2 = await Activate(client, body, idempKey);
        Assert.Equal(json1.Entitlement, json2.Entitlement);

        var modifiedBody = JsonSerializer.Serialize(new { key = "SBOT-different-key", device_jwk = devJwk, challenge = chal, proof }, JsonOptions);
        await AssertCode(Activate(client, modifiedBody, idempKey), 403, ErrorCodes.DeviceProofInvalid);
    }
}
