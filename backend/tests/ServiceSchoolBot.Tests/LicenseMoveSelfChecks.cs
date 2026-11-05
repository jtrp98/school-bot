using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using static ServiceSchoolBot.Tests.ApiV2LicenseDeviceTests;
using static ServiceSchoolBot.Tests.LicenseStoreSelfCheckFixture;

namespace ServiceSchoolBot.Tests;

public sealed class LicenseMoveSelfChecks
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Move_PreservesOldLeaseAndRejectsDetachedDevice(bool file)
    {
        using var fixture = new LicenseStoreSelfCheckFixture(file);
        var start = fixture.Clock.GetUtcNow();
        var created = await fixture.AdminA.CreateLicenseAsync(fixture.Admin,
            new CreateLicenseV2Request(100, "Test School", start.AddDays(10)));
        var (deviceA, jwkA) = CreateDeviceKey();
        var (deviceB, jwkB) = CreateDeviceKey();
        using var ownedA = deviceA;
        using var ownedB = deviceB;
        var leaseA = await fixture.Request(fixture.DeviceA, created.Key, deviceA, jwkA);
        await fixture.AssertLease(leaseA);
        Assert.Equal(start, leaseA.CheckedAt);
        Assert.Equal(start.AddSeconds(259200), leaseA.ExpiresAt);

        fixture.Clock.Advance(TimeSpan.FromHours(1));
        var original = (await fixture.Reader().GetLicenseByIdAsync(created.License.Id))!;
        var etag = OpaqueConcurrencyToken.ToETag(original.UpdateDate);
        var before = await fixture.Snapshot();
        var denied = await Assert.ThrowsAsync<DomainRuleException>(() => fixture.AdminB.UnbindLicenseAsync(
            LicenseAdminPrincipalStub.CreateStaffNoRole("P2"), original.SchoolBotLicenseId, etag, reason: "fixture:move"));
        Assert.Equal(ErrorCodes.StaffRoleRequired, denied.Code);
        EqualState(before, await fixture.Snapshot());

        await fixture.AdminB.UnbindLicenseAsync(fixture.Admin, original.SchoolBotLicenseId, etag, reason: "fixture:move");
        var detached = await fixture.Snapshot();
        var unbound = Assert.Single(detached.Licenses);
        var oldBinding = Assert.Single(detached.Bindings);
        Assert.Null(unbound.CurrentBindingId);
        Assert.Equal(original.BindingEpoch + 1, unbound.BindingEpoch);
        Assert.Equal(fixture.Clock.GetUtcNow(), oldBinding.DetachedAt);
        Assert.Equal(leaseA.CheckedAt, oldBinding.LastCheckedAt);
        Assert.Equal(leaseA.ExpiresAt, oldBinding.LastEntitlementExpiresAt);

        var leaseB = await fixture.Request(fixture.DeviceB, created.Key, deviceB, jwkB);
        await fixture.AssertLease(leaseB);
        Assert.Equal(fixture.Clock.GetUtcNow(), leaseB.CheckedAt);
        Assert.Equal(leaseB.CheckedAt.AddSeconds(259200), leaseB.ExpiresAt);
        var moved = await fixture.Snapshot();
        var current = Assert.Single(moved.Licenses);
        Assert.Equal(original.SchoolBotLicenseId, current.SchoolBotLicenseId);
        Assert.Equal(original.ExpiresAt, current.ExpiresAt);
        Assert.Equal(leaseB.BindingId, current.CurrentBindingId);
        Assert.Equal(oldBinding, moved.Bindings.Single(binding => binding.SchoolBotLicenseBindingId == leaseA.BindingId));
        var live = Assert.Single(moved.Bindings, binding => binding.DetachedAt is null);
        Assert.Equal(current.BindingEpoch, live.Epoch);

        fixture.Clock.Advance(TimeSpan.FromHours(1));
        Assert.Equal(ErrorCodes.BindingRevoked,
            await Outcome(fixture.Request(fixture.DeviceA, created.Key, deviceA, jwkA, leaseA.BindingId)));
        EqualState(moved, await fixture.Snapshot());
    }
}

