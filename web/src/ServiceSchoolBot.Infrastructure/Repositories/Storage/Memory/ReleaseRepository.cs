using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Releases;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed class ReleaseRepository(ISchoolBotBackingStore store) : IReleaseRepository
{
    private ICurriculumBackingStore Storage => store as ICurriculumBackingStore
        ?? throw new InvalidOperationException("Atomic release storage is required.");
    public async Task<IReadOnlyList<DesktopRelease>> ListAsync(CancellationToken ct = default) =>
        (await Storage.LoadCurriculumUnitAsync(ct)).Releases ?? [];

    public Task<DesktopRelease> CreateAsync(DesktopRelease release, CancellationToken ct = default) =>
        Storage.MutateCurriculumUnitAsync((snapshot, _) =>
        {
            var releases = snapshot.Releases ?? [];
            if (releases.Any(r => r.Version == release.Version && r.Channel == release.Channel && r.Platform == release.Platform))
                throw new DomainRuleException(ErrorCodes.ReleaseExists, "Release identity already exists.");
            return Task.FromResult(new CurriculumStoreMutation<DesktopRelease>(snapshot with { Releases = [.. releases, release] }, release));
        }, ct);

    public Task<DesktopRelease> PublishAsync(string id, CancellationToken ct = default) =>
        Storage.MutateCurriculumUnitAsync((snapshot, _) =>
        {
            var releases = snapshot.Releases ?? [];
            var original = releases.SingleOrDefault(r => r.Id == id)
                ?? throw new DomainRuleException(ErrorCodes.NotFound, "Release not found.");
            var published = original with { Published = true };
            return Task.FromResult(new CurriculumStoreMutation<DesktopRelease>(snapshot with
            { Releases = releases.Select(r => r.Id == id ? published : r).ToArray() }, published));
        }, ct);
}
