using ServiceSchoolBot.Domain.Licensing;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage;

public sealed partial class MemorySchoolBotBackingStore : IWebSessionBackingStore
{
    public async Task<T> MutateWebSessionUnitAsync<T>(
        Func<CurriculumStoreSnapshot, IReadOnlyList<SchoolBotLicenseRecord>, CurriculumStoreMutation<T>> mutate,
        CancellationToken ct)
    {
        await curriculumLock.WaitAsync(ct);
        try
        {
            lock (licenseLock)
            {
                var candidate = mutate(curriculumSnapshot, licenses.Values.ToArray());
                ct.ThrowIfCancellationRequested();
                if (candidate.Changed) curriculumSnapshot = candidate.Snapshot;
                return candidate.Result;
            }
        }
        finally { curriculumLock.Release(); }
    }
}
