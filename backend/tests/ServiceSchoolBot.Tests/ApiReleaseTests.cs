using ServiceSchoolBot.Domain;
using static ServiceSchoolBot.Tests.ApiHelpers;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.Tests;

public sealed class ApiReleaseTests
{
    public static TheoryData<string, string, string> InvalidReleaseFields => new()
    {
        { "version", "01.0.0", "INVALID_RELEASE" },
        { "version", "1.2", "INVALID_RELEASE" },
        { "version", "1.2.3-beta", "INVALID_RELEASE" },
        { "version", "1.2.3\n", "INVALID_RELEASE" },
        { "channel", "nightly", "INVALID_RELEASE" },
        { "platform", "linux-amd64", "INVALID_RELEASE" },
        { "url", "http://example.com/a.zip", "INVALID_ARTIFACT" },
        { "url", "https://user:secret@example.com/a.zip", "INVALID_ARTIFACT" },
        { "url", "not a url", "INVALID_ARTIFACT" },
        { "url", "https://example.com/" + new string('a', 2029), "INVALID_ARTIFACT" },
        { "sha256", new string('a', 63), "INVALID_ARTIFACT" },
        { "sha256", new string('g', 64), "INVALID_ARTIFACT" },
        { "signature", "", "INVALID_ARTIFACT" },
        { "signature", "   ", "INVALID_ARTIFACT" },
        { "signature", new string('s', 4097), "INVALID_ARTIFACT" }
    };

    private static Rpc.CheckUpdateRequest Check(string version, string channel = "stable",
        string platform = "windows-amd64") => new() { Version = version, Channel = channel, Platform = platform };

    [Theory]
    [MemberData(nameof(InvalidReleaseFields))]
    public async Task InvalidReleaseFieldsAreRejected(string field, string value, string code)
    {
        using var factory = new ApiFactory();
        var releases = GrpcTest.Releases(factory);
        var fields = ReleaseFields();
        switch (field)
        {
            case "version": fields.Version = value; break;
            case "channel": fields.Channel = value; break;
            case "platform": fields.Platform = value; break;
            case "url": fields.Url = value; break;
            case "sha256": fields.Sha256 = value; break;
            case "signature": fields.Signature = value; break;
        }
        await GrpcTest.AssertError(releases.CreateReleaseAsync(fields, Admin()), 400, code);
    }

    [Fact]
    public async Task ReleaseUrlAndSignatureAcceptTheirMaximumLength()
    {
        using var factory = new ApiFactory();
        var releases = GrpcTest.Releases(factory);
        var fields = ReleaseFields();
        fields.Url = "https://example.com/" + new string('a', 2028);
        fields.Signature = new string('s', 4096);
        var created = await releases.CreateReleaseAsync(fields, Admin());
        Assert.False(string.IsNullOrEmpty(created.Id));
    }

    [Fact]
    public async Task DuplicateReleaseIsConflictAndTrailingNewlineIsNotADifferentVersion()
    {
        using var factory = new ApiFactory();
        var releases = GrpcTest.Releases(factory);
        await CreateReleaseId(releases, ReleaseFields("1.2.3"));
        await GrpcTest.AssertError(releases.CreateReleaseAsync(ReleaseFields("1.2.3"), Admin()), 409, "RELEASE_EXISTS");
        await GrpcTest.AssertError(releases.CreateReleaseAsync(ReleaseFields("1.2.3\n"), Admin()), 400, "INVALID_RELEASE");
        await releases.CreateReleaseAsync(ReleaseFields("1.2.3", "beta"), Admin());
        await releases.CreateReleaseAsync(ReleaseFields("1.2.3", platform: "darwin-arm64"), Admin());
    }

    [Fact]
    public async Task PublishUnknownReleaseIsNotFound()
    {
        using var factory = new ApiFactory();
        var releases = GrpcTest.Releases(factory);
        await GrpcTest.AssertError(
            releases.PublishReleaseAsync(new Rpc.PublishReleaseRequest { Id = "missing" }, Admin()), 404, "NOT_FOUND");
    }

    [Fact]
    public async Task ManagingReleasesRequiresCentralStaff()
    {
        using var factory = new ApiFactory();
        var releases = GrpcTest.Releases(factory);
        var schoolUser = GrpcTest.Headers(new GatewayCaller(11, 21, ServiceSchoolBot.API.GrpcServices.GatewayIdentity.Employee));
        await GrpcTest.AssertError(releases.ListReleasesAsync(new Rpc.ListReleasesRequest(), GrpcTest.Headers()),
            403, ErrorCodes.StaffRoleRequired);
        await GrpcTest.AssertError(releases.CreateReleaseAsync(ReleaseFields(), schoolUser), 403, ErrorCodes.StaffRoleRequired);
        await GrpcTest.AssertError(releases.PublishReleaseAsync(new Rpc.PublishReleaseRequest { Id = "missing" }, schoolUser),
            403, ErrorCodes.StaffRoleRequired);
        Assert.Empty((await releases.ListReleasesAsync(new Rpc.ListReleasesRequest(), Admin())).Items);
    }

    [Theory]
    [InlineData("1.0.0", "stable", "linux-amd64")]
    [InlineData("1.0.0", "nightly", "windows-amd64")]
    [InlineData("latest", "stable", "windows-amd64")]
    [InlineData("1.0.0\n", "stable", "windows-amd64")]
    public async Task UpdateCheckWithInvalidValuesIsRejected(string version, string channel, string platform)
    {
        using var factory = new ApiFactory();
        var releases = GrpcTest.Releases(factory);
        await GrpcTest.AssertError(releases.CheckUpdateAsync(Check(version, channel, platform), GrpcTest.Headers()),
            400, "INVALID_QUERY");
    }

    [Fact]
    public async Task UpdateCheckReturnsTheHighestPublishedVersionNumerically()
    {
        using var factory = new ApiFactory();
        var releases = GrpcTest.Releases(factory);
        foreach (var version in new[] { "1.2.0", "1.10.0", "1.9.0" })
            await Publish(releases, await CreateReleaseId(releases, ReleaseFields(version)));
        await CreateReleaseId(releases, ReleaseFields("2.0.0"));

        var newest = await releases.CheckUpdateAsync(Check("1.0.0"), GrpcTest.Headers());
        Assert.NotNull(newest.Release);
        Assert.Equal("1.10.0", newest.Release.Version);
        Assert.False((await releases.CheckUpdateAsync(Check("1.10.0"), GrpcTest.Headers())).Release is not null);
    }

    [Fact]
    public async Task UpdateCheckIgnoresDraftOnlyReleases()
    {
        using var factory = new ApiFactory();
        var releases = GrpcTest.Releases(factory);
        await CreateReleaseId(releases, ReleaseFields("2.0.0"));
        var result = await releases.CheckUpdateAsync(Check("1.0.0"), GrpcTest.Headers());
        Assert.Null(result.Release);
    }
}
