using System.Collections.Concurrent;
using ServiceSchoolBot.Domain.Identity;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Domain.Projects;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage;

public sealed partial class MemorySchoolBotBackingStore : ISchoolBotBackingStore
{
    private readonly object licenseLock = new();
    private readonly ConcurrentDictionary<int, SchoolBotLicenseRecord> licenses = new();
    private readonly ConcurrentDictionary<int, SchoolBotLicenseBindingRecord> bindings = new();

    private readonly object projectLock = new();
    private IReadOnlyList<SchoolBotProjectRecord> projects = [];
    private IReadOnlyList<SchoolBotProjectRevisionRecord> revisions = [];

    public Task<LicenseStoreSnapshot> LoadLicenseUnitAsync(CancellationToken ct = default)
    {
        lock (licenseLock)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(new LicenseStoreSnapshot(licenses.Values.ToArray(), bindings.Values.ToArray()));
        }
    }

    public Task<T> MutateLicenseUnitAsync<T>(
        Func<LicenseStoreSnapshot, LicenseStoreMutation<T>> mutate, CancellationToken ct = default)
    {
        lock (licenseLock)
        {
            ct.ThrowIfCancellationRequested();
            var previous = new LicenseStoreSnapshot(licenses.Values.ToArray(), bindings.Values.ToArray());
            var candidate = mutate(previous);
            candidate.Snapshot.ValidateMutation(previous);
            ct.ThrowIfCancellationRequested();
            licenses.Clear();
            foreach (var item in candidate.Snapshot.Licenses) licenses[item.SchoolBotLicenseId] = item;
            bindings.Clear();
            foreach (var item in candidate.Snapshot.Bindings) bindings[item.SchoolBotLicenseBindingId] = item;
            return Task.FromResult(candidate.Result);
        }
    }

    public async Task<IReadOnlyList<SchoolBotLicenseRecord>> LoadLicensesAsync(CancellationToken ct = default) =>
        (await LoadLicenseUnitAsync(ct)).Licenses;

    public async Task<IReadOnlyList<SchoolBotLicenseBindingRecord>> LoadBindingsAsync(CancellationToken ct = default) =>
        (await LoadLicenseUnitAsync(ct)).Bindings;

    public Task<IReadOnlyList<SchoolBotProjectRecord>> LoadProjectsAsync(CancellationToken ct = default)
    {
        lock (projectLock)
        {
            return Task.FromResult<IReadOnlyList<SchoolBotProjectRecord>>(projects.ToArray());
        }
    }

    public Task<IReadOnlyList<SchoolBotProjectRevisionRecord>> LoadProjectRevisionsAsync(CancellationToken ct = default)
    {
        lock (projectLock)
        {
            return Task.FromResult<IReadOnlyList<SchoolBotProjectRevisionRecord>>(revisions.ToArray());
        }
    }

    public Task CommitProjectAsync(
        IReadOnlyList<SchoolBotProjectRecord> itemsProjects,
        IReadOnlyList<SchoolBotProjectRevisionRecord> itemsRevisions,
        CancellationToken ct = default)
    {
        var newProjects = itemsProjects.ToArray();
        var newRevisions = itemsRevisions.ToArray();
        lock (projectLock)
        {
            projects = newProjects;
            revisions = newRevisions;
        }
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<SchoolBotSessionRecord>> LoadSessionsAsync(CancellationToken ct = default) =>
        (await LoadCurriculumUnitAsync(ct)).Sessions ?? [];

    public Task SaveSessionsAsync(IReadOnlyList<SchoolBotSessionRecord> items, CancellationToken ct = default) =>
        MutateCurriculumUnitAsync((snapshot, token) => Task.FromResult(
            new CurriculumStoreMutation<bool>(snapshot with { Sessions = items.ToArray() }, true)), ct);
}
