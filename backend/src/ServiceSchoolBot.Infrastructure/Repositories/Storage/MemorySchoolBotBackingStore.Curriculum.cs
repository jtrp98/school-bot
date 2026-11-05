using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage;

public sealed partial class MemorySchoolBotBackingStore : ICurriculumBackingStore
{
    private readonly SemaphoreSlim curriculumLock = new(1, 1);
    private CurriculumStoreSnapshot curriculumSnapshot = CurriculumStoreSnapshot.Empty;

    public async Task<CurriculumStoreSnapshot> LoadCurriculumUnitAsync(CancellationToken ct = default)
    {
        await curriculumLock.WaitAsync(ct);
        try { return curriculumSnapshot; }
        finally { curriculumLock.Release(); }
    }

    public async Task<T> MutateCurriculumUnitAsync<T>(
        Func<CurriculumStoreSnapshot, CancellationToken, Task<CurriculumStoreMutation<T>>> mutate,
        CancellationToken ct = default)
    {
        await curriculumLock.WaitAsync(ct);
        try
        {
            var candidate = await mutate(curriculumSnapshot, ct);
            ct.ThrowIfCancellationRequested();
            if (candidate.Changed) curriculumSnapshot = candidate.Snapshot;
            return candidate.Result;
        }
        finally { curriculumLock.Release(); }
    }

    public async Task<IReadOnlyList<SchoolBotCurriculumRecord>> LoadCurriculaAsync(CancellationToken ct = default)
    {
        var snapshot = await LoadCurriculumUnitAsync(ct);
        return snapshot.Curricula ?? [];
    }

    public async Task<IReadOnlyList<SchoolBotLessonRecord>> LoadLessonsAsync(CancellationToken ct = default)
    {
        var snapshot = await LoadCurriculumUnitAsync(ct);
        return snapshot.Lessons ?? [];
    }

    public async Task<IReadOnlyList<SchoolBotMissionRecord>> LoadMissionsAsync(CancellationToken ct = default)
    {
        var snapshot = await LoadCurriculumUnitAsync(ct);
        return snapshot.Missions ?? [];
    }

    public async Task<IReadOnlyList<SchoolBotCriteriaRecord>> LoadCriteriaAsync(CancellationToken ct = default)
    {
        var snapshot = await LoadCurriculumUnitAsync(ct);
        return snapshot.Criteria ?? [];
    }

    public async Task<IReadOnlyList<SchoolBotActivityRecord>> LoadActivitiesAsync(CancellationToken ct = default)
    {
        var snapshot = await LoadCurriculumUnitAsync(ct);
        return snapshot.Activities ?? [];
    }

    public async Task<IReadOnlyList<SchoolBotAuditEventRecord>> LoadAuditEventsAsync(CancellationToken ct = default)
    {
        var snapshot = await LoadCurriculumUnitAsync(ct);
        return snapshot.AuditEvents ?? [];
    }

    public async Task CommitCurriculumUnitAsync(
        IReadOnlyList<SchoolBotCurriculumRecord> curricula,
        IReadOnlyList<SchoolBotLessonRecord> lessons,
        IReadOnlyList<SchoolBotMissionRecord> missions,
        IReadOnlyList<SchoolBotCriteriaRecord> criteria,
        IReadOnlyList<SchoolBotActivityRecord> activities,
        IReadOnlyList<SchoolBotAuditEventRecord> auditEvents,
        CancellationToken ct = default)
    {
        await MutateCurriculumUnitAsync((current, token) => Task.FromResult(
            new CurriculumStoreMutation<bool>(current with
            {
                Curricula = curricula,
                Lessons = lessons,
                Missions = missions,
                Criteria = criteria,
                Activities = activities,
                AuditEvents = auditEvents
            }, true)), ct);
    }
}
