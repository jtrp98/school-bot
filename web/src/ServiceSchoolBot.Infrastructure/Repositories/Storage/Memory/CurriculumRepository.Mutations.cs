using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed partial class CurriculumRepository
{
    public async Task<SchoolBotCurriculumRecord> CreateCurriculumAsync(
        SchoolBotCurriculumRecord curriculum,
        SchoolBotAuditEventRecord initialAudit,
        CancellationToken ct = default)
    {

        if (!state.isCurriculumTransaction)
            return await unitOfWork.CommitCurriculumMutationAsync(adapter =>
                adapter.CreateCurriculumAsync(curriculum, initialAudit, ct), ct);
        await state.curriculumSyncLock.WaitAsync(ct);
        try
        {
            await state.ReloadCurriculaUnderLockAsync(ct);
            var exists = state.curricula.Any(c => c.CurriculumKey == curriculum.CurriculumKey && c.Revision == curriculum.Revision && !c.IsDelete);
            if (exists)
                throw new DomainRuleException(ErrorCodes.StateConflict, "A curriculum with this key and revision already exists.");

            var curriculumId = state.nextCurriculumId++;
            var auditId = state.nextAuditEventId++;
            var created = curriculum with { SchoolBotCurriculumId = curriculumId };
            var audit = initialAudit with { SchoolBotAuditEventId = auditId, EntityId = curriculumId };

            var candidateCurricula = new List<SchoolBotCurriculumRecord>(state.curricula) { created };
            var candidateAudits = new List<SchoolBotAuditEventRecord>(state.auditEvents) { audit };

            await state.backingStore.CommitCurriculumUnitAsync(
                candidateCurricula,
                state.lessons,
                state.missions,
                state.criteria,
                state.activities,
                candidateAudits,
                ct);

            state.curricula.Add(created);
            state.auditEvents.Add(audit);
            return created;
        }
        finally { state.curriculumSyncLock.Release(); }
    }

    public async Task<SchoolBotCurriculumRecord> UpdateCurriculumMetadataAsync(
        int curriculumId,
        int levelId,
        string title,
        string sourceRef,
        IReadOnlyList<string> recommendedGrades,
        IReadOnlyList<CurriculumSubject> subjects,
        string expectedETag,
        SchoolBotAuditEventRecord auditEvent,
        CancellationToken ct = default)
    {

        if (!state.isCurriculumTransaction)
            return await unitOfWork.CommitCurriculumMutationAsync(adapter =>
                adapter.UpdateCurriculumMetadataAsync(curriculumId, levelId, title, sourceRef, recommendedGrades, subjects, expectedETag, auditEvent, ct), ct);
        await state.curriculumSyncLock.WaitAsync(ct);
        try
        {
            await state.ReloadCurriculaUnderLockAsync(ct);
            var index = state.curricula.FindIndex(c => c.SchoolBotCurriculumId == curriculumId && !c.IsDelete);
            if (index < 0)
                throw new DomainRuleException(ErrorCodes.NotFound, "Curriculum not found.");

            var existing = state.curricula[index];
            CurriculumService.ValidateStoredMetadata(existing, expectedETag);

            var now = state.clock.GetUtcNow();
            var auditId = state.nextAuditEventId++;
            var updated = existing with
            {
                LevelId = levelId,
                Title = title,
                SourceRef = sourceRef,
                RecommendedGrades = recommendedGrades,
                Subjects = subjects,
                UpdateDate = now
            };
            var audit = auditEvent with { SchoolBotAuditEventId = auditId, EntityId = curriculumId, CreateDate = now };

            var candidateCurricula = new List<SchoolBotCurriculumRecord>(state.curricula);
            candidateCurricula[index] = updated;
            var candidateAudits = new List<SchoolBotAuditEventRecord>(state.auditEvents) { audit };

            await state.backingStore.CommitCurriculumUnitAsync(
                candidateCurricula,
                state.lessons,
                state.missions,
                state.criteria,
                state.activities,
                candidateAudits,
                ct);

            state.curricula[index] = updated;
            state.auditEvents.Add(audit);
            return updated;
        }
        finally { state.curriculumSyncLock.Release(); }
    }

    public async Task<SchoolBotCurriculumRecord> UpdateCurriculumContentAsync(
        int curriculumId,
        IReadOnlyList<SchoolBotLessonRecord> newLessons,
        IReadOnlyList<SchoolBotMissionRecord> newMissions,
        IReadOnlyList<SchoolBotCriteriaRecord> newCriteria,
        IReadOnlyList<SchoolBotActivityRecord> newActivities,
        string expectedETag,
        SchoolBotAuditEventRecord auditEvent,
        CancellationToken ct = default)
    {

        if (!state.isCurriculumTransaction)
            return await unitOfWork.CommitCurriculumMutationAsync(adapter =>
                adapter.UpdateCurriculumContentAsync(curriculumId, newLessons, newMissions, newCriteria, newActivities, expectedETag, auditEvent, ct), ct);
        await state.curriculumSyncLock.WaitAsync(ct);
        try
        {
            await state.ReloadCurriculaUnderLockAsync(ct);
            var index = state.curricula.FindIndex(c => c.SchoolBotCurriculumId == curriculumId && !c.IsDelete);
            if (index < 0)
                throw new DomainRuleException(ErrorCodes.NotFound, "Curriculum not found.");

            var existing = state.curricula[index];
            CurriculumService.ValidateStoredContent(existing, expectedETag);

            var now = state.clock.GetUtcNow();
            var auditId = state.nextAuditEventId++;
            var updated = existing with { UpdateDate = now };
            var audit = auditEvent with { SchoolBotAuditEventId = auditId, EntityId = curriculumId, CreateDate = now };

            var candidateLessons = state.lessons.Where(l => l.CurriculumId != curriculumId).ToList();
            var existingLessonIds = state.lessons.Where(l => l.CurriculumId == curriculumId).Select(l => l.SchoolBotLessonId).ToHashSet();
            var candidateMissions = state.missions.Where(m => !existingLessonIds.Contains(m.LessonId)).ToList();
            var existingMissionIds = state.missions.Where(m => existingLessonIds.Contains(m.LessonId)).Select(m => m.SchoolBotMissionId).ToHashSet();
            var candidateCriteria = state.criteria.Where(c => !existingMissionIds.Contains(c.MissionId)).ToList();
            var candidateActivities = state.activities.Where(a => !existingLessonIds.Contains(a.LessonId)).ToList();

            var lessonIdMap = new Dictionary<int, int>();
            foreach (var lesson in newLessons)
            {
                var id = state.nextLessonId++;
                if (lesson.SchoolBotLessonId != 0) lessonIdMap[lesson.SchoolBotLessonId] = id;
                var importSource = state.lessons.FirstOrDefault(existingLesson =>
                    existingLesson.CurriculumId == curriculumId && existingLesson.LessonKey == lesson.LessonKey)?.ImportSource;
                candidateLessons.Add(lesson with { SchoolBotLessonId = id, CurriculumId = curriculumId, CreateDate = now, UpdateDate = now, ImportSource = importSource });
            }

            var missionIdMap = new Dictionary<int, int>();
            foreach (var mission in newMissions)
            {
                var id = state.nextMissionId++;
                if (mission.SchoolBotMissionId != 0) missionIdMap[mission.SchoolBotMissionId] = id;
                var realLessonId = lessonIdMap.TryGetValue(mission.LessonId, out var lid) ? lid : mission.LessonId;
                candidateMissions.Add(mission with { SchoolBotMissionId = id, LessonId = realLessonId, CreateDate = now, UpdateDate = now });
            }

            foreach (var crit in newCriteria)
            {
                var id = state.nextCriteriaId++;
                var realMissionId = missionIdMap.TryGetValue(crit.MissionId, out var mid) ? mid : crit.MissionId;
                candidateCriteria.Add(crit with { SchoolBotCriteriaId = id, MissionId = realMissionId, CreateDate = now, UpdateDate = now });
            }

            foreach (var act in newActivities)
            {
                var id = state.nextActivityId++;
                var realLessonId = lessonIdMap.TryGetValue(act.LessonId, out var lid) ? lid : act.LessonId;
                candidateActivities.Add(act with { SchoolBotActivityId = id, LessonId = realLessonId, CreateDate = now, UpdateDate = now });
            }

            var candidateCurricula = new List<SchoolBotCurriculumRecord>(state.curricula);
            candidateCurricula[index] = updated;
            var candidateAudits = new List<SchoolBotAuditEventRecord>(state.auditEvents) { audit };

            await state.backingStore.CommitCurriculumUnitAsync(
                candidateCurricula,
                candidateLessons,
                candidateMissions,
                candidateCriteria,
                candidateActivities,
                candidateAudits,
                ct);

            state.curricula[index] = updated;
            state.auditEvents.Add(audit);

            state.lessons.Clear();
            state.lessons.AddRange(candidateLessons);
            state.missions.Clear();
            state.missions.AddRange(candidateMissions);
            state.criteria.Clear();
            state.criteria.AddRange(candidateCriteria);
            state.activities.Clear();
            state.activities.AddRange(candidateActivities);

            return updated;
        }
        finally { state.curriculumSyncLock.Release(); }
    }

    public async Task<SchoolBotCurriculumRecord> PublishCurriculumAsync(
        int curriculumId,
        string approvalRef,
        string expectedETag,
        SchoolBotAuditEventRecord publishAudit,
        CancellationToken ct = default)
    {

        if (!state.isCurriculumTransaction)
            return await unitOfWork.CommitCurriculumMutationAsync(adapter =>
                adapter.PublishCurriculumAsync(curriculumId, approvalRef, expectedETag, publishAudit, ct), ct);
        await state.curriculumSyncLock.WaitAsync(ct);
        try
        {
            await state.ReloadCurriculaUnderLockAsync(ct);
            var index = state.curricula.FindIndex(c => c.SchoolBotCurriculumId == curriculumId && !c.IsDelete);
            if (index < 0)
                throw new DomainRuleException(ErrorCodes.NotFound, "Curriculum not found.");

            var existing = state.curricula[index];

            if (CurriculumService.IsStoredPublicationReplay(
                existing, state.auditEvents, approvalRef, expectedETag, curriculumId))
                return existing;

            var now = state.clock.GetUtcNow();
            var auditId = state.nextAuditEventId++;
            var published = existing with
            {
                Lifecycle = "published",
                PublishedAt = now,
                UpdateDate = now
            };

            var audit = publishAudit with
            {
                SchoolBotAuditEventId = auditId,
                EntityId = curriculumId,
                CreateDate = now,
                Reason = approvalRef
            };

            var candidateCurricula = new List<SchoolBotCurriculumRecord>(state.curricula);
            candidateCurricula[index] = published;
            var candidateAudits = new List<SchoolBotAuditEventRecord>(state.auditEvents) { audit };

            await state.backingStore.CommitCurriculumUnitAsync(
                candidateCurricula,
                state.lessons,
                state.missions,
                state.criteria,
                state.activities,
                candidateAudits,
                ct);

            state.curricula[index] = published;
            state.auditEvents.Add(audit);
            return published;
        }
        finally { state.curriculumSyncLock.Release(); }
    }
}
