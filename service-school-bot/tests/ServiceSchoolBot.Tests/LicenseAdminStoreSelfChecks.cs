using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using System.Text.Json;
using ServiceSchoolBot.Domain;
using static ServiceSchoolBot.Tests.LicenseStoreSelfCheckFixture;

namespace ServiceSchoolBot.Tests;

public sealed class LicenseAdminStoreSelfChecks
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateList_SeparateServices_FreshReaderRetainsMetadata(bool file)
    {
        using var fixture = new LicenseStoreSelfCheckFixture(file);
        var (_, created) = await fixture.Create();
        var view = Assert.Single(await fixture.AdminB.ListLicensesAsync(fixture.Admin));
        Assert.Equal(created.SchoolBotLicenseId, view.Id);
        Assert.Equal(0, view.ActivatedDevices);
        Assert.Equal(created, await fixture.Reader().GetLicenseByIdAsync(view.Id));
        var reopened = new LicenseAdminService(fixture.Reader(), fixture.Clock);
        Assert.Equal(view, Assert.Single(await reopened.ListLicensesAsync(fixture.Admin)));
        var json = JsonSerializer.Serialize(view);
        Assert.DoesNotContain("KeyHash", json);
        Assert.DoesNotContain("DeviceJkt", json);
        Assert.Null(Assert.Single((await fixture.Snapshot()).Licenses).CurrentBindingId);
        Assert.Empty((await fixture.Snapshot()).Bindings);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DeniedOrInvalidCreate_FreshFixtures_DoNotChangeDomain(bool file)
    {
        for (var scenario = 0; scenario < 10; scenario++)
        {
            using var fixture = new LicenseStoreSelfCheckFixture(file);
            var auth = scenario switch
            {
                0 => LicenseAdminPrincipalStub.CreateUnconfigured(),
                1 => LicenseAdminPrincipalStub.CreateAnonymous(),
                2 => LicenseAdminPrincipalStub.CreateExpired(),
                3 => LicenseAdminPrincipalStub.CreateWrongChannel(),
                4 => LicenseAdminPrincipalStub.CreateStaffNoRole(),
                5 => LicenseAdminPrincipalStub.CreateStaffOtherRole(),
                _ => fixture.Admin
            };
            var request = new CreateLicenseV2Request(100,
                scenario == 6 ? " " : "Test School",
                scenario == 7 ? fixture.Clock.GetUtcNow() : fixture.Clock.GetUtcNow().AddDays(1),
                scenario == 8 ? 2 : 1);
            if (scenario == 9) request = request with { SchoolId = 0 };
            var before = await fixture.Snapshot();
            await Assert.ThrowsAsync<DomainRuleException>(() => fixture.AdminA.CreateLicenseAsync(auth, request));
            EqualState(before, await fixture.Snapshot());
        }
    }
}
