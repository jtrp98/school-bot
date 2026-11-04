using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed class CurriculumImportRepository : ICurriculumImportRepository
{
    private readonly SchoolBotRepositoryState state;

    public CurriculumImportRepository(SchoolBotUnitOfWork unitOfWork)
    {
        state = unitOfWork.State;
    }

    public async Task<CurriculumImportResponse> ImportCurriculumLessonsAsync(
        int curriculumId,
        IReadOnlyList<PreparedCurriculumImportRow> rows,
        SchoolBotAuditEventRecord audit,
        CancellationToken ct = default)
    {
        if (!state.isCurriculumTransaction)
            throw new InvalidOperationException("Imports require an idempotent commit transaction.");
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(audit);
        await state.curriculumSyncLock.WaitAsync(ct);
        try
        {
            await state.ReloadCurriculaUnderLockAsync(ct);
            var curriculum = state.curricula.FirstOrDefault(item => item.SchoolBotCurriculumId == curriculumId && !item.IsDelete)
                ?? throw new DomainRuleException(ErrorCodes.NotFound, "Curriculum not found.");
            var (results, created) = CurriculumService.PrepareStoredImportRows(
                curriculumId, curriculum, state.lessons, rows);
            if (created.Count == 0)
                return new(curriculumId, curriculum.UpdateDate!.Value, results);

            var now = state.clock.GetUtcNow();
            var candidateLessons = new List<SchoolBotLessonRecord>(state.lessons);
            var candidateMissions = new List<SchoolBotMissionRecord>(state.missions);
            var candidateCriteria = new List<SchoolBotCriteriaRecord>(state.criteria);
            var candidateActivities = new List<SchoolBotActivityRecord>(state.activities);
            foreach (var row in created)
            {
                var lesson = row.Lesson;
                var lessonId = state.nextLessonId++;
                candidateLessons.Add(new SchoolBotLessonRecord(lessonId, curriculumId, lesson.LessonKey,
                    lesson.Ordinal, lesson.Title, lesson.Objective, lesson.Instructions,
                    false, null, now, null, now, null, row.ImportSource));
                var resultIndex = results.FindIndex(result => result.SourceRowKey == lesson.LessonKey);
                results[resultIndex] = results[resultIndex] with { LessonId = lessonId };
                foreach (var mission in lesson.Missions ?? [])
                {
                    var missionId = state.nextMissionId++;
                    candidateMissions.Add(new SchoolBotMissionRecord(missionId, lessonId, mission.MissionKey,
                        mission.Ordinal, mission.Title, mission.Instructions, mission.Method,
                        false, null, now, null, now, null));
                    if (mission.Criteria is { } input)
                        candidateCriteria.Add(new SchoolBotCriteriaRecord(state.nextCriteriaId++, missionId,
                            input.Revision, input.Rules, input.SourceRef, input.ApprovedBy, input.ApprovedAt,
                            false, null, now, null, now, null));
                }
                foreach (var activity in lesson.Activities ?? [])
                    candidateActivities.Add(new SchoolBotActivityRecord(state.nextActivityId++, lessonId, activity.ActivityKey,
                        activity.Ordinal, activity.Title, activity.Type, activity.Mode, activity.Indicators ?? "[]",
                        activity.Config ?? "{}", false, null, now, null, now, null));
            }
            var candidateCurricula = state.curricula.Select(item => item.SchoolBotCurriculumId == curriculumId
                ? item with { UpdateDate = now } : item).ToArray();
            var candidateAudits = new List<SchoolBotAuditEventRecord>(state.auditEvents)
            {
                audit with { SchoolBotAuditEventId = state.nextAuditEventId++, EntityId = curriculumId, CreateDate = now }
            };
            await state.backingStore.CommitCurriculumUnitAsync(candidateCurricula, candidateLessons, candidateMissions,
                candidateCriteria, candidateActivities, candidateAudits, ct);
            return new(curriculumId, now, results);
        }
        finally { state.curriculumSyncLock.Release(); }
    }

}
