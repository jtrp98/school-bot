using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed partial class CurriculumRepository : ICurriculumRepository, IIdempotentSchoolBotCommit
{
    private readonly SchoolBotUnitOfWork unitOfWork;
    private readonly SchoolBotRepositoryState state;

    public CurriculumRepository(SchoolBotUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
        state = unitOfWork.State;
    }

    public Task<IdempotentCommitResult<T>> CommitWithIdempotencyAsync<T>(
        IdempotencyCommand command,
        Func<ISchoolBotRepositoryScope, int?, DateTimeOffset, CancellationToken, Task> recheckAccess,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<MutationResult<T>>> mutate,
        CancellationToken ct = default) =>
        unitOfWork.CommitWithIdempotencyAsync(command, recheckAccess, mutate, ct);
    public async Task<SchoolBotCurriculumRecord?> GetCurriculumByIdAsync(int curriculumId, CancellationToken ct = default)
    {
        await state.curriculumSyncLock.WaitAsync(ct);
        try
        {
            await state.ReloadCurriculaUnderLockAsync(ct);
            return state.curricula.FirstOrDefault(c => c.SchoolBotCurriculumId == curriculumId && !c.IsDelete);
        }
        finally { state.curriculumSyncLock.Release(); }
    }

    public async Task<SchoolBotCurriculumRecord?> GetCurriculumByKeyAndRevisionAsync(Guid curriculumKey, int revision, CancellationToken ct = default)
    {
        await state.curriculumSyncLock.WaitAsync(ct);
        try
        {
            await state.ReloadCurriculaUnderLockAsync(ct);
            return state.curricula.FirstOrDefault(c => c.CurriculumKey == curriculumKey && c.Revision == revision && !c.IsDelete);
        }
        finally { state.curriculumSyncLock.Release(); }
    }

    public async Task<IReadOnlyList<SchoolBotCurriculumRecord>> ListCurriculaAsync(int? levelId = null, string? lifecycle = null, CancellationToken ct = default)
    {
        await state.curriculumSyncLock.WaitAsync(ct);
        try
        {
            await state.ReloadCurriculaUnderLockAsync(ct);
            return state.curricula
                .Where(c => !c.IsDelete &&
                            (!levelId.HasValue || c.LevelId == levelId.Value) &&
                            (string.IsNullOrWhiteSpace(lifecycle) || string.Equals(c.Lifecycle, lifecycle, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(c => c.LevelId)
                .ThenBy(c => c.SchoolBotCurriculumId)
                .ToArray();
        }
        finally { state.curriculumSyncLock.Release(); }
    }

    public async Task<IReadOnlyList<SchoolBotLessonRecord>> ListLessonsByCurriculumIdAsync(int curriculumId, CancellationToken ct = default)
    {
        await state.curriculumSyncLock.WaitAsync(ct);
        try
        {
            await state.ReloadCurriculaUnderLockAsync(ct);
            return state.lessons
                .Where(l => l.CurriculumId == curriculumId && !l.IsDelete)
                .OrderBy(l => l.Ordinal)
                .ToArray();
        }
        finally { state.curriculumSyncLock.Release(); }
    }

    public async Task<IReadOnlyList<SchoolBotMissionRecord>> ListMissionsByLessonIdAsync(int lessonId, CancellationToken ct = default)
    {
        await state.curriculumSyncLock.WaitAsync(ct);
        try
        {
            await state.ReloadCurriculaUnderLockAsync(ct);
            return state.missions
                .Where(m => m.LessonId == lessonId && !m.IsDelete)
                .OrderBy(m => m.Ordinal)
                .ToArray();
        }
        finally { state.curriculumSyncLock.Release(); }
    }

    public async Task<IReadOnlyList<SchoolBotCriteriaRecord>> ListCriteriaByMissionIdAsync(int missionId, CancellationToken ct = default)
    {
        await state.curriculumSyncLock.WaitAsync(ct);
        try
        {
            await state.ReloadCurriculaUnderLockAsync(ct);
            return state.criteria
                .Where(c => c.MissionId == missionId && !c.IsDelete)
                .OrderBy(c => c.Revision)
                .ToArray();
        }
        finally { state.curriculumSyncLock.Release(); }
    }

    public async Task<IReadOnlyList<SchoolBotActivityRecord>> ListActivitiesByLessonIdAsync(int lessonId, CancellationToken ct = default)
    {
        await state.curriculumSyncLock.WaitAsync(ct);
        try
        {
            await state.ReloadCurriculaUnderLockAsync(ct);
            return state.activities
                .Where(a => a.LessonId == lessonId && !a.IsDelete)
                .OrderBy(a => a.Ordinal)
                .ToArray();
        }
        finally { state.curriculumSyncLock.Release(); }
    }

    public async Task<IReadOnlyList<SchoolBotAuditEventRecord>> ListAuditEventsByEntityAsync(string entityType, int entityId, CancellationToken ct = default)
    {
        await state.curriculumSyncLock.WaitAsync(ct);
        try
        {
            await state.ReloadCurriculaUnderLockAsync(ct);
            return state.auditEvents
                .Where(a => !a.IsDelete &&
                            string.Equals(a.EntityType, entityType, StringComparison.OrdinalIgnoreCase) &&
                            a.EntityId == entityId)
                .OrderBy(a => a.CreateDate)
                .ToArray();
        }
        finally { state.curriculumSyncLock.Release(); }
    }

}
