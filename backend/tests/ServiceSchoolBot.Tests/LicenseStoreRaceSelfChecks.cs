using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using static ServiceSchoolBot.Tests.ApiV2LicenseDeviceTests;
using static ServiceSchoolBot.Tests.LicenseStoreSelfCheckFixture;

namespace ServiceSchoolBot.Tests;

public sealed class LicenseStoreRaceSelfChecks
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NewActivationRace_TwoAdapters_OneLiveBinding(bool file)
    {
        for (var round = 0; round < 50; round++)
        {
            using var fixture = new LicenseStoreSelfCheckFixture(file);
            var (key, initial) = await fixture.Create();
            var (a, jwkA) = CreateDeviceKey();
            var (b, jwkB) = CreateDeviceKey();
            using var ownedA = a;
            using var ownedB = b;
            fixture.PauseA.Arm();
            fixture.PauseB.Arm();
            var requestA = fixture.Request(fixture.DeviceA, key, a, jwkA);
            var requestB = fixture.Request(fixture.DeviceB, key, b, jwkB);
            await Task.WhenAll(fixture.PauseA.Ready.Task, fixture.PauseB.Ready.Task).WaitAsync(TimeSpan.FromSeconds(15));
            fixture.PauseA.Release();
            fixture.PauseB.Release();
            var outcomes = await Task.WhenAll(Outcome(requestA), Outcome(requestB));
            Assert.Equal(1, outcomes.Count(result => result == "200"));
            Assert.Equal(1, outcomes.Count(result => result == ErrorCodes.DeviceLimit));
            var snapshot = await fixture.Snapshot();
            var binding = Assert.Single(snapshot.Bindings);
            var license = Assert.Single(snapshot.Licenses);
            Assert.Equal(binding.SchoolBotLicenseBindingId, license.CurrentBindingId);
            Assert.Equal(initial.BindingEpoch, license.BindingEpoch);
            Assert.Null(binding.DetachedAt);
            await fixture.AssertLease(outcomes[0] == "200" ? await requestA : await requestB);
            var winner = outcomes[0] == "200" ? (a, jwkA, fixture.DeviceA) : (b, jwkB, fixture.DeviceB);
            await fixture.Request(winner.Item3, key, winner.Item1, winner.Item2);
            var validated = await fixture.Request(winner.Item3, key, winner.Item1, winner.Item2, binding.SchoolBotLicenseBindingId);
            await fixture.AssertLease(validated);
            Assert.Equal(license, await fixture.Reader().GetLicenseByIdAsync(license.SchoolBotLicenseId));
            Assert.Single((await fixture.Snapshot()).Bindings);
        }
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public async Task PausedLeaseWriter_AdminCommitWins_NoStaleMutation(bool file, bool activate, bool revoke)
    {
        using var fixture = new LicenseStoreSelfCheckFixture(file);
        var (key, license) = await fixture.Create();
        var (device, jwk) = CreateDeviceKey();
        using var ownedDevice = device;
        var first = await fixture.Request(fixture.DeviceA, key, device, jwk);
        fixture.Clock.Advance(TimeSpan.FromSeconds(3));
        fixture.PauseA.Arm();
        var pending = fixture.Request(fixture.DeviceA, key, device, jwk, activate ? null : first.BindingId);
        await fixture.PauseA.Ready.Task.WaitAsync(TimeSpan.FromSeconds(15));
        var current = await fixture.AdapterB.GetLicenseByIdAsync(license.SchoolBotLicenseId);
        var etag = OpaqueConcurrencyToken.ToETag(current!.UpdateDate);
        if (revoke)
            await fixture.AdminB.RevokeLicenseAsync(fixture.Admin, license.SchoolBotLicenseId, etag, "Test revoke");
        else
            await fixture.AdminB.UnbindLicenseAsync(fixture.Admin, license.SchoolBotLicenseId, etag, reason: "Test unbind");
        var afterAdmin = await fixture.Snapshot();
        fixture.PauseA.Release();
        Assert.Equal(revoke ? ErrorCodes.LicenseInactive : ErrorCodes.BindingRevoked, await Outcome(pending));
        EqualState(afterAdmin, await fixture.Snapshot());
        Assert.Equal(first.ExpiresAt, Assert.Single(afterAdmin.Bindings).LastEntitlementExpiresAt);
        if (!revoke)
            Assert.Equal(ErrorCodes.DeviceLimit, await Outcome(fixture.Request(fixture.DeviceA, key, device, jwk)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LeaseWins_UnbindRetainsHistory_AdminCanAuthorizeNewRow(bool file)
    {
        using var fixture = new LicenseStoreSelfCheckFixture(file);
        var (key, initial) = await fixture.Create();
        var (a, jwkA) = CreateDeviceKey();
        var (b, jwkB) = CreateDeviceKey();
        using var ownedA = a;
        using var ownedB = b;
        var first = await fixture.Request(fixture.DeviceA, key, a, jwkA);
        fixture.Clock.Advance(TimeSpan.FromMinutes(1));
        var lease = await fixture.Request(fixture.DeviceA, key, a, jwkA, first.BindingId);
        await fixture.AssertLease(lease);
        var current = await fixture.AdapterB.GetLicenseByIdAsync(initial.SchoolBotLicenseId);
        await fixture.AdminB.UnbindLicenseAsync(fixture.Admin, initial.SchoolBotLicenseId,
            OpaqueConcurrencyToken.ToETag(current!.UpdateDate), reason: "Test move");
        Assert.Equal(ErrorCodes.DeviceLimit, await Outcome(fixture.Request(fixture.DeviceA, key, a, jwkA)));
        var moved = await fixture.Request(fixture.DeviceB, key, b, jwkB);
        await fixture.AssertLease(moved);
        var beforeReturn = await fixture.Snapshot();
        var oldA = beforeReturn.Bindings.Single(row => row.SchoolBotLicenseBindingId == first.BindingId);
        Assert.Equal(lease.ExpiresAt, oldA.LastEntitlementExpiresAt);
        Assert.NotNull(oldA.DetachedAt);
        current = await fixture.AdapterA.GetLicenseByIdAsync(initial.SchoolBotLicenseId);
        await fixture.AdminA.UnbindLicenseAsync(fixture.Admin, initial.SchoolBotLicenseId,
            OpaqueConcurrencyToken.ToETag(current!.UpdateDate), DeviceProofVerifier.ComputeDeviceJkt(jwkA), "Test return");
        var returned = await fixture.Request(fixture.DeviceA, key, a, jwkA);
        await fixture.AssertLease(returned);
        var snapshot = await fixture.Snapshot();
        Assert.Equal(3, snapshot.Bindings.Count);
        Assert.Equal(oldA, snapshot.Bindings.Single(row => row.SchoolBotLicenseBindingId == first.BindingId));
        Assert.Equal(1, snapshot.Bindings.Count(row => row.DetachedAt is null));
        Assert.NotEqual(first.BindingId, returned.BindingId);
        Assert.Equal(initial.BindingEpoch + 2, Assert.Single(snapshot.Licenses).BindingEpoch);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExpiryBoundary_ActivateAndValidate_RejectWithoutWrites(bool file)
    {
        foreach (var seconds in new[] { -1, 0, 1 })
        {
            using var fixture = new LicenseStoreSelfCheckFixture(file);
            var (key, license) = await fixture.Create();
            var (device, jwk) = CreateDeviceKey();
            using var owned = device;
            var first = await fixture.Request(fixture.DeviceA, key, device, jwk);
            fixture.Clock.Advance(license.ExpiresAt - fixture.Clock.GetUtcNow() + TimeSpan.FromSeconds(seconds));
            var before = await fixture.Snapshot();
            var activate = fixture.Request(fixture.DeviceA, key, device, jwk);
            Assert.Equal(seconds < 0 ? "200" : ErrorCodes.LicenseInactive, await Outcome(activate));
            var validate = fixture.Request(fixture.DeviceB, key, device, jwk, first.BindingId);
            Assert.Equal(seconds < 0 ? "200" : ErrorCodes.LicenseInactive, await Outcome(validate));
            if (seconds < 0)
            {
                await fixture.AssertLease(await activate);
                await fixture.AssertLease(await validate);
            }
            else EqualState(before, await fixture.Snapshot());
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StaleEtag_TwoAdminServices_SameClock_NoMutation(bool file)
    {
        using var fixture = new LicenseStoreSelfCheckFixture(file);
        var (_, initial) = await fixture.Create();
        var etag = OpaqueConcurrencyToken.ToETag(initial.UpdateDate);
        await fixture.AdminB.UnbindLicenseAsync(fixture.Admin, initial.SchoolBotLicenseId, etag, reason: "First");
        var before = await fixture.Snapshot();
        var error = await Assert.ThrowsAsync<DomainRuleException>(() => fixture.AdminA.UnbindLicenseAsync(
            fixture.Admin, initial.SchoolBotLicenseId, etag, reason: "Stale"));
        Assert.Equal(ErrorCodes.VersionConflict, error.Code);
        EqualState(before, await fixture.Snapshot());
    }
}
