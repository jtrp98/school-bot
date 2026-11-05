using ServiceSchoolBot.Domain.Projects;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage;

internal sealed class SchoolBotRepositoryState
{
    internal readonly ISchoolBotBackingStore backingStore;
    internal readonly TimeProvider clock;
    internal static readonly SemaphoreSlim syncLock = new(1, 1);

    internal readonly List<SchoolBotProjectRecord> projects = [];
    internal readonly List<SchoolBotProjectRevisionRecord> revisions = [];

    internal int nextProjectId = 1;
    internal int nextRevisionId = 1;
    internal bool initialized;

    internal SchoolBotRepositoryState(ISchoolBotBackingStore backingStore, TimeProvider? clock = null)
    {
        this.backingStore = backingStore;
        this.clock = clock ?? TimeProvider.System;
    }

    internal async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (initialized) return;
        await syncLock.WaitAsync(ct);
        try
        {
            if (initialized) return;
            await ReloadUnderLockAsync(ct);
            initialized = true;
        }
        finally
        {
            syncLock.Release();
        }
    }

    internal async Task ReloadUnderLockAsync(CancellationToken ct)
    {
        var loadedProjects = await backingStore.LoadProjectsAsync(ct);
        var loadedRevisions = await backingStore.LoadProjectRevisionsAsync(ct);

        projects.Clear();
        projects.AddRange(loadedProjects);
        nextProjectId = projects.Count > 0 ? projects.Max(p => p.SchoolBotProjectId) + 1 : 1;

        revisions.Clear();
        revisions.AddRange(loadedRevisions);
        nextRevisionId = revisions.Count > 0 ? revisions.Max(r => r.SchoolBotProjectRevisionId) + 1 : 1;
    }

    internal readonly List<SchoolBotCurriculumRecord> curricula = [];
    internal readonly List<SchoolBotLessonRecord> lessons = [];
    internal readonly List<SchoolBotMissionRecord> missions = [];
    internal readonly List<SchoolBotCriteriaRecord> criteria = [];
    internal readonly List<SchoolBotActivityRecord> activities = [];
    internal readonly List<SchoolBotAuditEventRecord> auditEvents = [];

    internal int nextCurriculumId = 1;
    internal int nextLessonId = 1;
    internal int nextMissionId = 1;
    internal int nextCriteriaId = 1;
    internal int nextActivityId = 1;
    internal int nextAuditEventId = 1;

    internal async Task ReloadCurriculaUnderLockAsync(CancellationToken ct)
    {
        var snapshot = backingStore is ICurriculumBackingStore store
            ? await store.LoadCurriculumUnitAsync(ct)
            : new CurriculumStoreSnapshot(1,
                await backingStore.LoadCurriculaAsync(ct), await backingStore.LoadLessonsAsync(ct),
                await backingStore.LoadMissionsAsync(ct), await backingStore.LoadCriteriaAsync(ct),
                await backingStore.LoadActivitiesAsync(ct), await backingStore.LoadAuditEventsAsync(ct));
        var loadedCurricula = snapshot.Curricula;
        var loadedLessons = snapshot.Lessons;
        var loadedMissions = snapshot.Missions;
        var loadedCriteria = snapshot.Criteria;
        var loadedActivities = snapshot.Activities;
        var loadedAuditEvents = snapshot.AuditEvents;

        curricula.Clear();
        curricula.AddRange(loadedCurricula);
        nextCurriculumId = curricula.Count > 0 ? curricula.Max(c => c.SchoolBotCurriculumId) + 1 : 1;

        lessons.Clear();
        lessons.AddRange(loadedLessons);
        nextLessonId = lessons.Count > 0 ? lessons.Max(l => l.SchoolBotLessonId) + 1 : 1;

        missions.Clear();
        missions.AddRange(loadedMissions);
        nextMissionId = missions.Count > 0 ? missions.Max(m => m.SchoolBotMissionId) + 1 : 1;

        criteria.Clear();
        criteria.AddRange(loadedCriteria);
        nextCriteriaId = criteria.Count > 0 ? criteria.Max(c => c.SchoolBotCriteriaId) + 1 : 1;

        activities.Clear();
        activities.AddRange(loadedActivities);
        nextActivityId = activities.Count > 0 ? activities.Max(a => a.SchoolBotActivityId) + 1 : 1;

        auditEvents.Clear();
        auditEvents.AddRange(loadedAuditEvents);
        nextAuditEventId = auditEvents.Count > 0 ? auditEvents.Max(a => a.SchoolBotAuditEventId) + 1 : 1;
    }

    internal readonly SemaphoreSlim curriculumSyncLock = new(1, 1);
    internal bool isCurriculumTransaction;

    internal ICurriculumBackingStore CurriculumStore => backingStore as ICurriculumBackingStore
        ?? throw new InvalidOperationException("The backing store does not support atomic curriculum commits.");
}
