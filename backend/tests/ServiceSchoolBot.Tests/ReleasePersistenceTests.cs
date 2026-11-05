using ServiceSchoolBot.Application.Services.Implementations.Management;
using ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Tests;

public sealed class ReleasePersistenceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PublishedReleaseSurvivesNewServiceAndDuplicateVersionIsRejected(bool file)
    {
        using var fixture = new CourseRunFixture(file);
        await fixture.Initialize();
        var service = new ReleaseService(new ReleaseRepository(fixture.Store()));
        var created = await service.CreateAsync("1.2.3", "stable", "windows-amd64", "https://downloads.example.test/app.msi",
            new string('a', 64), "signature");
        await service.PublishAsync(created.Id);
        var restarted = new ReleaseService(new ReleaseRepository(fixture.Store()));
        Assert.Equal(created.Id, (await restarted.CheckAsync("1.0.0", "stable", "windows-amd64"))!.Id);
        Assert.Equal(ErrorCodes.ReleaseExists, (await Assert.ThrowsAsync<DomainRuleException>(() => restarted.CreateAsync("1.2.3", "stable",
            "windows-amd64", "https://downloads.example.test/app.msi", new string('a', 64), "signature"))).Code);
    }
}
