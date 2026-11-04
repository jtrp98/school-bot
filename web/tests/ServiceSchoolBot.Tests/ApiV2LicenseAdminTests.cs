using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.ApiHelpers;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.Tests;

public sealed class ApiV2LicenseAdminTests
{
    private static string Future(ApiFactory factory, int days = 30) =>
        factory.Clock.GetUtcNow().AddDays(days).ToString("O");

    private static Rpc.CreateLicenseRequest License(int schoolId, string schoolName, string expiresAt,
        int? deviceLimit = null, Guid? idempotencyKey = null)
    {
        var request = new Rpc.CreateLicenseRequest { SchoolId = schoolId, SchoolName = schoolName, ExpiresAt = expiresAt };
        if (deviceLimit is { } limit) request.DeviceLimit = limit;
        if (idempotencyKey is { } key) request.IdempotencyKey = key.ToString("D");
        return request;
    }

    private static readonly Rpc.ListLicensesRequest ListAll = new();

    [Fact]
    public async Task Ac02701_UnconfiguredStaffAuth_Returns503()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var headers = GrpcTest.As(LicenseAdminPrincipalStub.ValidAdminToken, ("X-Staff-Auth-Unconfigured", "1"));
        await GrpcTest.AssertError(client.ListLicensesAsync(ListAll, headers), 503, ErrorCodes.AuthNotConfigured);
    }

    [Fact]
    public async Task Ac02701_MissingOrInvalidToken_Returns401()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        // 1. No caller identity
        await GrpcTest.AssertError(client.ListLicensesAsync(ListAll, GrpcTest.Headers()), 401, ErrorCodes.Unauthorized);

        // 2. Unknown credential
        await GrpcTest.AssertError(client.ListLicensesAsync(ListAll, GrpcTest.As("unknown-random-token")),
            401, ErrorCodes.Unauthorized);
    }

    [Fact]
    public async Task Ac02701_ExpiredStaffSession_Returns401SessionExpired()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        await GrpcTest.AssertError(client.ListLicensesAsync(ListAll, GrpcTest.As(LicenseAdminPrincipalStub.ExpiredToken)),
            401, ErrorCodes.SessionExpired);
    }

    [Fact]
    public async Task Ac02701_WrongChannel_Returns403ChannelNotAllowed()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        await GrpcTest.AssertError(client.ListLicensesAsync(ListAll, GrpcTest.As(LicenseAdminPrincipalStub.WrongChannelToken)),
            403, ErrorCodes.ChannelNotAllowed);
    }

    [Fact]
    public async Task Ac02701_StaffWithoutActiveLicenseAdminRole_Returns403StaffRoleRequired()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        // 1. Staff session with no roles
        await GrpcTest.AssertError(client.ListLicensesAsync(ListAll, GrpcTest.As(LicenseAdminPrincipalStub.NoRoleToken)),
            403, ErrorCodes.StaffRoleRequired);

        // 2. Staff session with a different role (release_publisher only)
        await GrpcTest.AssertError(client.ListLicensesAsync(ListAll, GrpcTest.As(LicenseAdminPrincipalStub.OtherRoleToken)),
            403, ErrorCodes.StaffRoleRequired);
    }

    [Fact]
    public async Task Ac02702_CreateLicenseWithValidData_Returns201WithSingleDeviceKey()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var expiresAt = factory.Clock.GetUtcNow().AddDays(60);
        var json = await client.CreateLicenseAsync(
            License(101, "School Bright Demonstration Academy", expiresAt.ToString("O")), AdminV2());

        var key = json.Key;
        Assert.StartsWith("SBOT-", key);
        Assert.True(key.Length >= 32);

        var license = json.License;
        Assert.True(license.Id > 0);
        Assert.Equal(101, license.SchoolId);
        Assert.Equal("School Bright Demonstration Academy", license.SchoolName);
        Assert.False(license.Revoked);
        Assert.Equal(0, license.ActivatedDevices);

        var returnedExpiry = DateTimeOffset.Parse(license.ExpiresAt);
        Assert.Equal(TimeSpan.Zero, returnedExpiry.Offset);
        Assert.Equal(expiresAt.UtcDateTime, returnedExpiry.UtcDateTime);
    }

    [Theory]
    [InlineData("", "INVALID_LICENSE")]
    [InlineData("   ", "INVALID_LICENSE")]
    public async Task Ac02702_BlankSchoolName_IsRejected(string schoolName, string expectedCode)
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        await GrpcTest.AssertError(client.CreateLicenseAsync(License(101, schoolName, Future(factory)), AdminV2()),
            400, expectedCode);
    }

    [Fact]
    public async Task Ac02702_SchoolNameLongerThan200Chars_IsRejected()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        await GrpcTest.AssertError(client.CreateLicenseAsync(License(101, new string('s', 201), Future(factory)), AdminV2()),
            400, "INVALID_LICENSE");
    }

    [Fact]
    public async Task Ac02702_PastOrCurrentExpiry_IsRejected()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);
        var now = factory.Clock.GetUtcNow();

        foreach (var past in new[] { now.AddMinutes(-5), now })
        {
            await GrpcTest.AssertError(client.CreateLicenseAsync(License(101, "Valid School", past.ToString("O")), AdminV2()),
                400, "INVALID_LICENSE");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(100)]
    public async Task Ac02702_MultiDeviceOrInvalidLimit_IsRejected(int limit)
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        await GrpcTest.AssertError(
            client.CreateLicenseAsync(License(101, "Valid School", Future(factory), limit), AdminV2()),
            400, "INVALID_LICENSE");
    }

    [Fact]
    public async Task Ac02703_ListLicenses_ReturnsMetadataWithoutLeakingKeyOrDeviceIds()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        // Create a license first
        var createdKey = (await client.CreateLicenseAsync(
            License(202, "Metadata Protection School", Future(factory)), AdminV2())).Key;

        // Fetch listing
        var list = await client.ListLicensesAsync(ListAll, AdminV2());

        var rawJson = list.ToString();
        Assert.DoesNotContain(createdKey, rawJson);
        Assert.DoesNotContain("key_hash", rawJson);
        Assert.DoesNotContain("keyHash", rawJson);
        Assert.DoesNotContain("device_jkt", rawJson);
        Assert.DoesNotContain("deviceJkt", rawJson);
        Assert.DoesNotContain("device_ids", rawJson);
        Assert.DoesNotContain("deviceIds", rawJson);

        Assert.NotEmpty(list.Licenses);
        var found = list.Licenses.FirstOrDefault(n => n.SchoolName == "Metadata Protection School");
        Assert.NotNull(found);

        Assert.Equal(202, found.SchoolId);
        Assert.False(found.Revoked);
        Assert.Equal(0, found.ActivatedDevices);
        Assert.False(string.IsNullOrEmpty(found.KeyHint));
        Assert.Null(Rpc.LicenseItem.Descriptor.FindFieldByName("key"));
    }

    [Fact]
    public async Task Ac02704_RestartApi_RetainsLicenseAndStatusData()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "SBOT_RestartTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            int createdLicenseId;
            string keyHint;

            // Host instance 1: create license on file backing store
            using (var factory1 = new ApiFactory())
            using (var configured1 = factory1.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                    services.AddSingleton<ISchoolBotBackingStore>(_ => new FileSchoolBotBackingStore(tempDir)))))
            {
                var client1 = GrpcTest.Licenses(configured1);
                var json1 = await client1.CreateLicenseAsync(License(303, "Persistent Academy",
                    factory1.Clock.GetUtcNow().AddDays(45).ToString("O")), AdminV2());

                createdLicenseId = json1.License.Id;
                keyHint = json1.License.KeyHint;
            }

            // Host instance 2 (simulating service restart): reads the same file backing store
            using (var factory2 = new ApiFactory())
            using (var configured2 = factory2.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                    services.AddSingleton<ISchoolBotBackingStore>(_ => new FileSchoolBotBackingStore(tempDir)))))
            {
                var client2 = GrpcTest.Licenses(configured2);
                var list = await client2.ListLicensesAsync(ListAll, AdminV2());

                var restored = list.Licenses.FirstOrDefault(n => n.Id == createdLicenseId);
                Assert.NotNull(restored);

                Assert.Equal(303, restored.SchoolId);
                Assert.Equal("Persistent Academy", restored.SchoolName);
                Assert.Equal(keyHint, restored.KeyHint);
                Assert.False(restored.Revoked);
                Assert.Equal(0, restored.ActivatedDevices);
            }
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    [Fact]
    public async Task Decision046_IdempotencyReplayWithin15Minutes_RecoversPlainKey()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        var requestKey = Guid.NewGuid();
        var payload = License(505, "Replay Recovery School", Future(factory), idempotencyKey: requestKey);

        // First call with Idempotency-Key
        var json1 = await client.CreateLicenseAsync(payload, AdminV2());
        var initialKey = json1.Key;
        var licenseId = json1.License.Id;

        // Second call with same Idempotency-Key and same payload (within 15 minutes)
        var json2 = await client.CreateLicenseAsync(payload, AdminV2());
        var replayedKey = json2.Key;
        var replayedId = json2.License.Id;

        // Recovered key must match the original key exactly per Decision 046
        Assert.Equal(initialKey, replayedKey);
        Assert.Equal(licenseId, replayedId);

        // Calling with same Idempotency-Key but different payload triggers 409 IDEMPOTENCY_CONFLICT
        await GrpcTest.AssertError(client.CreateLicenseAsync(
            License(505, "Different School Name", Future(factory), idempotencyKey: requestKey), AdminV2()),
            409, ErrorCodes.IdempotencyConflict);

        // Advance clock past 15 minutes (900 seconds) -> 409 IDEMPOTENCY_EXPIRED
        factory.Clock.Advance(TimeSpan.FromSeconds(901));

        await GrpcTest.AssertError(client.CreateLicenseAsync(payload, AdminV2()), 409, ErrorCodes.IdempotencyExpired);
    }

    [Fact]
    public async Task RevokeAndUnbind_UpdateStatusAndSetETagHeader()
    {
        using var factory = new ApiFactory();
        var client = GrpcTest.Licenses(factory);

        // Create license
        var created = await client.CreateLicenseAsync(License(606, "Lifecycle Management School", Future(factory)), AdminV2());
        var id = created.License.Id;
        var initialETag = OpaqueConcurrencyToken.ToETag(DateTimeOffset.Parse(created.License.UpdateDate));

        // Unbind with wrong If-Match -> 412 PreconditionFailed
        await GrpcTest.AssertError(client.UnbindLicenseAsync(new Rpc.UnbindLicenseRequest
        {
            Id = id,
            IfMatch = "\"wrong-etag-value\"",
            Reason = "test mismatch"
        }, AdminV2()), 412, ErrorCodes.VersionConflict);

        // Unbind with correct If-Match
        var unbindRes = await client.UnbindLicenseAsync(new Rpc.UnbindLicenseRequest
        {
            Id = id,
            IfMatch = initialETag,
            Reason = "device replacement"
        }, AdminV2());
        Assert.True(unbindRes.HasEtag);
        var unbindETag = unbindRes.Etag;
        Assert.Equal(0, unbindRes.License.ActivatedDevices);

        // Revoke with correct ETag from unbind
        var revokeRes = await client.RevokeLicenseAsync(new Rpc.RevokeLicenseRequest
        {
            Id = id,
            IfMatch = unbindETag,
            Reason = "subscription ended"
        }, AdminV2());
        Assert.True(revokeRes.HasEtag);
        Assert.True(revokeRes.License.Revoked);
    }
}
