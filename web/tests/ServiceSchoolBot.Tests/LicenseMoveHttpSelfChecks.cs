using Rpc = SchoolBot.Infrastructure.Protos;
using static ServiceSchoolBot.Tests.IndependentLicenseGateTests;
using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using static ServiceSchoolBot.Tests.ApiHelpers;
using static ServiceSchoolBot.Tests.ApiV2LicenseDeviceTests;
using static ServiceSchoolBot.Tests.LicenseStoreSelfCheckFixture;

namespace ServiceSchoolBot.Tests;

public sealed class LicenseMoveHttpSelfChecks
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Move_UsesGrpcAndPersistsState(bool file)
    {
        using var fixture = new LicenseStoreSelfCheckFixture(file);
        using var baseFactory = new ApiFactory();
        using var factory = baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.AddSingleton<TimeProvider>(fixture.Clock);
            services.AddSingleton<ILicenseAdminService>(fixture.AdminB);
            services.AddSingleton<ILicenseDeviceService>(fixture.DeviceB);
        }));
        var client = GrpcTest.Licenses(factory);
        var start = fixture.Clock.GetUtcNow();
        var created = await fixture.AdminA.CreateLicenseAsync(fixture.Admin,
            new CreateLicenseV2Request(100, "Test School", start.AddDays(10)));
        var (keyA, jwkA) = CreateDeviceKey();
        var (keyB, jwkB) = CreateDeviceKey();
        using var ownedA = keyA;
        using var ownedB = keyB;

        var responseA = await DeviceRequest(client, created.Key, keyA, jwkA);
        Assert.Equal(200, responseA.HttpStatus);
        var leaseA = Lease(responseA.Response!);
        await fixture.AssertLease(leaseA);
        Assert.Equal(start, leaseA.CheckedAt);
        Assert.Equal(start.AddSeconds(259200), leaseA.ExpiresAt);

        fixture.Clock.Advance(TimeSpan.FromHours(1));
        var original = (await fixture.Reader().GetLicenseByIdAsync(created.License.Id))!;
        var etag = OpaqueConcurrencyToken.ToETag(original.UpdateDate);
        var before = await fixture.Snapshot();
        await GrpcTest.AssertError(client.UnbindLicenseAsync(Unbind(original.SchoolBotLicenseId, etag),
            AdminV2(LicenseAdminPrincipalStub.NoRoleToken)), 403, ErrorCodes.StaffRoleRequired);
        EqualState(before, await fixture.Snapshot());

        await client.UnbindLicenseAsync(Unbind(original.SchoolBotLicenseId, etag), AdminV2());
        var detached = await fixture.Snapshot();
        var unbound = Assert.Single(detached.Licenses);
        var oldBinding = Assert.Single(detached.Bindings);
        Assert.Null(unbound.CurrentBindingId);
        Assert.Equal(original.BindingEpoch + 1, unbound.BindingEpoch);
        Assert.Equal(fixture.Clock.GetUtcNow(), oldBinding.DetachedAt);
        Assert.Equal(leaseA.CheckedAt, oldBinding.LastCheckedAt);
        Assert.Equal(leaseA.ExpiresAt, oldBinding.LastEntitlementExpiresAt);

        var responseB = await DeviceRequest(client, created.Key, keyB, jwkB);
        Assert.Equal(200, responseB.HttpStatus);
        var leaseB = Lease(responseB.Response!);
        await fixture.AssertLease(leaseB);
        Assert.Equal(fixture.Clock.GetUtcNow(), leaseB.CheckedAt);
        Assert.Equal(leaseB.CheckedAt.AddSeconds(259200), leaseB.ExpiresAt);
        var moved = await fixture.Snapshot();
        var current = Assert.Single(moved.Licenses);
        Assert.Equal(original.SchoolBotLicenseId, current.SchoolBotLicenseId);
        Assert.Equal(original.ExpiresAt, current.ExpiresAt);
        Assert.Equal(leaseB.BindingId, current.CurrentBindingId);
        Assert.Equal(oldBinding, moved.Bindings.Single(binding => binding.SchoolBotLicenseBindingId == leaseA.BindingId));
        Assert.Equal(current.BindingEpoch, Assert.Single(moved.Bindings, binding => binding.DetachedAt is null).Epoch);

        fixture.Clock.Advance(TimeSpan.FromHours(1));
        var rejected = await DeviceRequest(client, created.Key, keyA, jwkA, leaseA.BindingId);
        GrpcTest.AssertError(rejected.Error!, 403, ErrorCodes.BindingRevoked);
        Assert.Null(rejected.Response);
        EqualState(moved, await fixture.Snapshot());
    }

    private static Rpc.UnbindLicenseRequest Unbind(int id, string etag) => new()
    {
        Id = id,
        IfMatch = etag,
        Reason = "fixture:move"
    };

    private static EntitlementResult Lease(Rpc.EntitlementResponse response) =>
        new(response.Valid, response.LicenseId, response.BindingId, response.Entitlement,
            DateTimeOffset.Parse(response.CheckedAt), DateTimeOffset.Parse(response.ExpiresAt));
}
