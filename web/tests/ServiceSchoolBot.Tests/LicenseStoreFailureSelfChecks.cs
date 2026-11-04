using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using System.Text.Json;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.ApiV2LicenseDeviceTests;
using static ServiceSchoolBot.Tests.LicenseStoreSelfCheckFixture;

namespace ServiceSchoolBot.Tests;

public sealed class LicenseStoreFailureSelfChecks
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SignerThrowsBeforePublish_LeavesOriginalGeneration(bool file)
    {
        using var fixture = new LicenseStoreSelfCheckFixture(file);
        var (key, _) = await fixture.Create();
        var (device, jwk) = CreateDeviceKey();
        using var ownedDevice = device;
        var before = await fixture.Snapshot();
        var service = new LicenseDeviceService(fixture.ProxyA, new EntitlementFailureSigner(fixture.Signer), fixture.Clock);
        await Assert.ThrowsAsync<IOException>(() => fixture.Request(service, key, device, jwk));
        EqualState(before, await fixture.Snapshot());
        var activated = await fixture.Request(fixture.DeviceB, key, device, jwk);
        await fixture.AssertLease(activated);
        before = await fixture.Snapshot();
        await Assert.ThrowsAsync<IOException>(() => fixture.Request(service, key, device, jwk, activated.BindingId));
        EqualState(before, await fixture.Snapshot());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MutationThrowsUnderStoreLock_LeavesOriginalGeneration(bool file)
    {
        using var fixture = new LicenseStoreSelfCheckFixture(file);
        await fixture.Create();
        var before = await fixture.Snapshot();
        await Assert.ThrowsAsync<IOException>(() => fixture.BackingStore.MutateLicenseUnitAsync<bool>(snapshot =>
        {
            _ = snapshot with { Licenses = [] };
            throw new IOException("Injected before publish");
        }));
        EqualState(before, await fixture.Snapshot());
    }

    [Fact]
    public async Task FilePublishFailsAfterCandidatePreparation_ReopenSeesOriginalGeneration()
    {
        using var fixture = new LicenseStoreSelfCheckFixture(true);
        var (_, license) = await fixture.Create();
        var before = await fixture.Snapshot();
        FileStream? heldSnapshot = null;
        try
        {
            var error = await Record.ExceptionAsync(() => fixture.BackingStore.MutateLicenseUnitAsync(snapshot =>
            {
                var changed = license with { RevokedAt = fixture.Clock.GetUtcNow() };
                heldSnapshot = new FileStream(Path.Combine(fixture.DirectoryPath!, "licenses_snapshot.json"),
                    FileMode.Open, FileAccess.Read, FileShare.Read);
                return new LicenseStoreMutation<bool>(snapshot with { Licenses = [changed] }, true);
            }));
            Assert.True(error is IOException or UnauthorizedAccessException);
        }
        finally { heldSnapshot?.Dispose(); }
        EqualState(before, await fixture.Snapshot());
        Assert.Empty(Directory.GetFiles(fixture.DirectoryPath!, "*.tmp"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TwoValidates_ClockAndLeaseDoNotMoveBackward(bool file)
    {
        using var fixture = new LicenseStoreSelfCheckFixture(file);
        var (key, _) = await fixture.Create();
        var (device, jwk) = CreateDeviceKey();
        using var ownedDevice = device;
        var activated = await fixture.Request(fixture.DeviceA, key, device, jwk);
        fixture.PauseA.Arm();
        var earlier = fixture.Request(fixture.DeviceA, key, device, jwk, activated.BindingId);
        await fixture.PauseA.Ready.Task.WaitAsync(TimeSpan.FromSeconds(15));
        fixture.Clock.Advance(TimeSpan.FromSeconds(5));
        var later = await fixture.Request(fixture.DeviceB, key, device, jwk, activated.BindingId);
        fixture.PauseA.Release();
        var committedLater = await earlier;
        Assert.True(committedLater.CheckedAt >= later.CheckedAt);
        Assert.True(committedLater.ExpiresAt >= later.ExpiresAt);
        await fixture.AssertLease(committedLater);
    }

    private sealed class EntitlementFailureSigner(ILicenseTokenSigner inner) : ILicenseTokenSigner
    {
        public string KeyId => inner.KeyId;
        public string Sign(string typ, object payload) => typ == "schoolbot-entitlement+jwt"
            ? throw new IOException("Injected signer failure") : inner.Sign(typ, payload);
        public bool Verify(string token, out JsonDocument? header, out JsonDocument? payload) =>
            inner.Verify(token, out header, out payload);
    }
}
