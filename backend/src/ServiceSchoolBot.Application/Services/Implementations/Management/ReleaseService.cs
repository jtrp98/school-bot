using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Releases;

namespace ServiceSchoolBot.Application.Services.Implementations.Management;

public sealed class ReleaseService(IReleaseRepository repository)
{
    public Task<IReadOnlyList<DesktopRelease>> ListAsync(CancellationToken ct = default) => repository.ListAsync(ct);

    public Task<DesktopRelease> CreateAsync(string version, string channel, string platform, string url,
        string sha256, string signature, CancellationToken ct = default) =>
        repository.CreateAsync(DesktopRelease.Create(version, channel, platform, url, sha256, signature), ct);

    public Task<DesktopRelease> PublishAsync(string id, CancellationToken ct = default) => repository.PublishAsync(id, ct);

    public async Task<DesktopRelease?> CheckAsync(string version, string channel, string platform, CancellationToken ct = default)
    {
        if (!DesktopRelease.ValidVersion(version) || !DesktopRelease.ValidChannel(channel) || !DesktopRelease.ValidPlatform(platform))
            throw new DomainRuleException(ErrorCodes.InvalidQuery, "Invalid update query.");
        return (await repository.ListAsync(ct)).Where(r => r.Published && r.Channel == channel && r.Platform == platform &&
            DesktopRelease.Newer(r.Version, version)).OrderByDescending(r => Version.Parse(r.Version)).FirstOrDefault();
    }
}
