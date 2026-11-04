using Rpc = SchoolBot.Infrastructure.Protos;
using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.ApiHelpers;
using static ServiceSchoolBot.Tests.ApiV2LicenseDeviceTests;

namespace ServiceSchoolBot.Tests;

public sealed class IndependentLicenseNormalTests
{
    private static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-11-03T00:00:00Z");

    private static async Task<(string Key, int Id)> Create(LicenseStoreSelfCheckFixture fixture)
    {
        var result = await fixture.AdminA.CreateLicenseAsync(fixture.Admin,
            new CreateLicenseV2Request(100, "S1", Start.AddDays(10), 1), Guid.NewGuid());
        return (result.Key, result.License.Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateAndListAcrossAdapters_ExposeMetadataAndLiveDeviceCount(bool file)
    {
        using var fixture = new LicenseStoreSelfCheckFixture(file);
        var (key, id) = await Create(fixture);
        var before = await fixture.Snapshot();
        var license = Assert.Single(before.Licenses);
        Assert.Equal(id, license.SchoolBotLicenseId);
        Assert.Null(license.CurrentBindingId);
        Assert.Empty(before.Bindings);
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(key))), license.KeyHash);
        Assert.Equal(0, Assert.Single(await fixture.AdminB.ListLicensesAsync(fixture.Admin)).ActivatedDevices);
        LicenseStoreSelfCheckFixture.EqualState(before, await fixture.Snapshot());
        var (device, jwk) = CreateDeviceKey();
        using var ownedDevice = device;
        await fixture.Request(fixture.DeviceA, key, device, jwk);
        var activated = await fixture.Snapshot();
        var list = await fixture.AdminB.ListLicensesAsync(fixture.Admin);
        var view = Assert.Single(list);
        Assert.Equal(id, view.Id);
        Assert.Equal("S1", view.SchoolName);
        Assert.Equal(Start.AddDays(10), view.ExpiresAt);
        Assert.Equal(1, view.ActivatedDevices);
        var json = JsonSerializer.Serialize(list);
        Assert.DoesNotContain(key, json);
        Assert.DoesNotContain(license.KeyHash, json);
        Assert.DoesNotContain(Assert.Single(activated.Bindings).DeviceJkt, json);
        Assert.DoesNotContain("KeyHash", json);
        Assert.DoesNotContain("DeviceIds", json);
        LicenseStoreSelfCheckFixture.EqualState(activated, await fixture.Snapshot());
    }

    [Fact]
    public async Task ReopenFileStoreAfterDisposingServices_RetainsUnboundLicense()
    {
        string directory;
        string expected;
        int id;
        directory = Path.Combine(AppContext.BaseDirectory, "IndependentNormalReopen_" + Guid.NewGuid().ToString("N"));
        {
            var store = new FileSchoolBotBackingStore(directory);
            var clock = new FakeClock(Start);
            var service = new LicenseAdminService(new SchoolBotUnitOfWork(store, clock), clock);
            var admin = LicenseAdminPrincipalStub.CreateLicenseAdmin(validFrom: Start.AddDays(-1));
            var result = await service.CreateLicenseAsync(admin,
                new CreateLicenseV2Request(100, "S1", Start.AddDays(10), 1), Guid.NewGuid());
            id = result.License.Id;
            expected = JsonSerializer.Serialize(await store.LoadLicenseUnitAsync());
        }
        var reopened = new FileSchoolBotBackingStore(directory);
        var reader = new SchoolBotUnitOfWork(reopened, new FakeClock(Start));
        var reopenedService = new LicenseAdminService(reader, new FakeClock(Start));
        var reopenedAdmin = LicenseAdminPrincipalStub.CreateLicenseAdmin(validFrom: Start.AddDays(-1));
        var view = Assert.Single(await reopenedService.ListLicensesAsync(reopenedAdmin));
        Assert.Equal(id, view.Id);
        Assert.False(view.Revoked);
        Assert.Equal(0, view.ActivatedDevices);
        Assert.Equal(expected, JsonSerializer.Serialize(await reopened.LoadLicenseUnitAsync()));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ActivateTwiceThenValidate_RetainsOneBindingAndRejectsSecondDevice(bool file)
    {
        using var fixture = new LicenseStoreSelfCheckFixture(file);
        var (key, id) = await Create(fixture);
        var (deviceA, jwkA) = CreateDeviceKey();
        var (deviceB, jwkB) = CreateDeviceKey();
        using var ownedA = deviceA;
        using var ownedB = deviceB;
        var empty = await fixture.Snapshot();
        var missing = await Assert.ThrowsAsync<DomainRuleException>(() =>
            fixture.Request(fixture.DeviceA, key, deviceA, jwkA, 999));
        Assert.Equal(ErrorCodes.DeviceNotActivated, missing.Code);
        LicenseStoreSelfCheckFixture.EqualState(empty, await fixture.Snapshot());
        var first = await fixture.Request(fixture.DeviceA, key, deviceA, jwkA);
        Assert.Equal(Start, first.CheckedAt);
        Assert.Equal(Start.AddDays(3), first.ExpiresAt);
        await fixture.AssertLease(first);
        var firstState = await fixture.Snapshot();
        fixture.Clock.Advance(TimeSpan.FromHours(1));
        var second = await fixture.Request(fixture.DeviceB, key, deviceA, jwkA);
        Assert.Equal(first.BindingId, second.BindingId);
        Assert.Equal(Start.AddHours(1), second.CheckedAt);
        Assert.Equal(Start.AddDays(3).AddHours(1), second.ExpiresAt);
        await fixture.AssertLease(second);
        fixture.Clock.Advance(TimeSpan.FromMinutes(1));
        var validated = await fixture.Request(fixture.DeviceA, key, deviceA, jwkA, first.BindingId);
        await fixture.AssertLease(validated);
        var state = await fixture.Snapshot();
        var binding = Assert.Single(state.Bindings);
        var original = Assert.Single(firstState.Bindings);
        Assert.Equal(original, binding with
        {
            LastCheckedAt = original.LastCheckedAt,
            LastEntitlementExpiresAt = original.LastEntitlementExpiresAt,
            UpdateDate = original.UpdateDate
        });
        var license = Assert.Single(state.Licenses);
        Assert.Equal(id, license.SchoolBotLicenseId);
        Assert.Equal(first.BindingId, license.CurrentBindingId);
        Assert.Equal(Assert.Single(empty.Licenses).BindingEpoch, license.BindingEpoch);
        var denied = await Assert.ThrowsAsync<DomainRuleException>(() =>
            fixture.Request(fixture.DeviceB, key, deviceB, jwkB));
        Assert.Equal(ErrorCodes.DeviceLimit, denied.Code);
        LicenseStoreSelfCheckFixture.EqualState(state, await fixture.Snapshot());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task InactiveLicense_RejectsActivateAndValidateWithoutChangingLease(bool file, bool revoke)
    {
        using var fixture = new LicenseStoreSelfCheckFixture(file);
        var (key, id) = await Create(fixture);
        var (device, jwk) = CreateDeviceKey();
        using var ownedDevice = device;
        var active = await fixture.Request(fixture.DeviceA, key, device, jwk);
        if (revoke)
            await fixture.AdminB.RevokeLicenseAsync(fixture.Admin, id, reason: "สิ้นสุดสิทธิ์");
        else
            fixture.Clock.Advance(TimeSpan.FromDays(11));
        var baseline = await fixture.Snapshot();
        foreach (var bindingId in new int?[] { null, active.BindingId })
        {
            var error = await Assert.ThrowsAsync<DomainRuleException>(() =>
                fixture.Request(fixture.DeviceB, key, device, jwk, bindingId));
            Assert.Equal(ErrorCodes.LicenseInactive, error.Code);
            LicenseStoreSelfCheckFixture.EqualState(baseline, await fixture.Snapshot());
        }
    }

    [Theory]
    [InlineData(false, "unconfigured", 503, "AUTH_NOT_CONFIGURED")]
    [InlineData(false, "anonymous", 401, "UNAUTHORIZED")]
    [InlineData(false, "no-role", 403, "STAFF_ROLE_REQUIRED")]
    [InlineData(true, "unconfigured", 503, "AUTH_NOT_CONFIGURED")]
    [InlineData(true, "anonymous", 401, "UNAUTHORIZED")]
    [InlineData(true, "no-role", 403, "STAFF_ROLE_REQUIRED")]
    public async Task AdminBoundary_RejectsUnprivilegedAccessWithoutCreatingLicense(
        bool file, string actor, int expectedStatus, string code)
    {
        using var fixture = new LicenseStoreSelfCheckFixture(file);
        using var host = new ApiFactory();
        host.Clock.Advance(Start - host.Clock.GetUtcNow());
        using var configured = host.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.AddSingleton(fixture.BackingStore)));
        var client = GrpcTest.Licenses(configured);
        var caller = actor switch
        {
            "anonymous" => GrpcTest.Headers(),
            "no-role" => GrpcTest.As(LicenseAdminPrincipalStub.NoRoleToken),
            _ => GrpcTest.As(LicenseAdminPrincipalStub.ValidAdminToken, ("X-Staff-Auth-Unconfigured", "1"))
        };
        var baseline = await fixture.Snapshot();
        await GrpcTest.AssertError(client.ListLicensesAsync(new Rpc.ListLicensesRequest(), caller), expectedStatus, code);
        await GrpcTest.AssertError(client.CreateLicenseAsync(new Rpc.CreateLicenseRequest
        {
            SchoolId = 100,
            SchoolName = "S1",
            ExpiresAt = Start.AddDays(10).ToString("O")
        }, caller), expectedStatus, code);
        LicenseStoreSelfCheckFixture.EqualState(baseline, await fixture.Snapshot());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AdminGrpcCreateAndList_WithoutDisclosingKey(bool file)
    {
        using var fixture = new LicenseStoreSelfCheckFixture(file);
        using var host = new ApiFactory();
        host.Clock.Advance(Start - host.Clock.GetUtcNow());
        using var configured = host.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.AddSingleton(fixture.BackingStore)));
        var client = GrpcTest.Licenses(configured);
        var created = await client.CreateLicenseAsync(new Rpc.CreateLicenseRequest
        {
            SchoolId = 100,
            SchoolName = "S1",
            ExpiresAt = Start.AddDays(10).ToString("O"),
            DeviceLimit = 1
        }, AdminV2());
        var key = created.Key;
        Assert.StartsWith("SBOT-", key);
        var listing = await client.ListLicensesAsync(new Rpc.ListLicensesRequest(), AdminV2());
        Assert.Equal(0, Assert.Single(listing.Licenses).ActivatedDevices);
        Assert.DoesNotContain(key, listing.ToString());
        var state = await fixture.Snapshot();
        Assert.Single(state.Licenses);
        Assert.Empty(state.Bindings);
        Assert.Null(Assert.Single(state.Licenses).CurrentBindingId);
    }
}



