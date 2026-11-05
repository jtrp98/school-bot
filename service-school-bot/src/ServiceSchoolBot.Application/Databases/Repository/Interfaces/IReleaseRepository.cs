using ServiceSchoolBot.Domain.Releases;

namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface IReleaseRepository
{
    Task<IReadOnlyList<DesktopRelease>> ListAsync(CancellationToken ct = default);
    Task<DesktopRelease> CreateAsync(DesktopRelease release, CancellationToken ct = default);
    Task<DesktopRelease> PublishAsync(string id, CancellationToken ct = default);
}
