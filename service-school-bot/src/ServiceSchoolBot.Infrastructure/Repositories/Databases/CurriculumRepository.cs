using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed partial class CurriculumRepository(SchoolBotDbContext db, SchoolBotUnitOfWork unitOfWork) : ICurriculumRepository, IIdempotentSchoolBotCommit
{
    public async Task<SchoolBotCurriculumRecord?> GetCurriculumByIdAsync(
        int curriculumId, CancellationToken ct = default)
    {
        var row = await db.SchoolBotCurricula.AsNoTracking()
            .Where(r => r.SchoolBotCurriculumId == curriculumId && !r.IsDelete)
            .SingleOrDefaultAsync(ct);
        return row is null ? null : From(row);
    }

    public async Task<SchoolBotCurriculumRecord?> GetCurriculumByKeyAndRevisionAsync(
        Guid curriculumKey, int revision, CancellationToken ct = default)
    {
        var row = await db.SchoolBotCurricula.AsNoTracking()
            .Where(r => r.CurriculumKey == curriculumKey && r.Revision == revision && !r.IsDelete)
            .SingleOrDefaultAsync(ct);
        return row is null ? null : From(row);
    }

    public async Task<IReadOnlyList<SchoolBotCurriculumRecord>> ListCurriculaAsync(
        int? levelId = null, string? lifecycle = null, CancellationToken ct = default)
    {
        var rows = await db.SchoolBotCurricula.AsNoTracking()
            .Where(r => !r.IsDelete && (levelId == null || r.LevelId == levelId) && (lifecycle == null || r.Lifecycle == lifecycle))
            .OrderBy(r => r.LevelId)
            .ToArrayAsync(ct);
        return rows.Select(From).ToArray();
    }

    public async Task<IReadOnlyList<SchoolBotLessonRecord>> ListLessonsByCurriculumIdAsync(
        int curriculumId, CancellationToken ct = default)
    {
        var rows = await db.SchoolBotLessons.AsNoTracking()
            .Where(r => r.CurriculumId == curriculumId && !r.IsDelete)
            .OrderBy(r => r.Ordinal)
            .ToArrayAsync(ct);
        return rows.Select(From).ToArray();
    }

    public async Task<IReadOnlyList<SchoolBotMissionRecord>> ListMissionsByLessonIdAsync(
        int lessonId, CancellationToken ct = default)
    {
        var rows = await db.SchoolBotMissions.AsNoTracking()
            .Where(r => r.LessonId == lessonId && !r.IsDelete)
            .OrderBy(r => r.Ordinal)
            .ToArrayAsync(ct);
        return rows.Select(From).ToArray();
    }

    public async Task<IReadOnlyList<SchoolBotCriteriaRecord>> ListCriteriaByMissionIdAsync(
        int missionId, CancellationToken ct = default)
    {
        var rows = await db.SchoolBotCriteria.AsNoTracking()
            .Where(r => r.MissionId == missionId && !r.IsDelete)
            .OrderBy(r => r.Revision)
            .ToArrayAsync(ct);
        return rows.Select(From).ToArray();
    }

    public async Task<IReadOnlyList<SchoolBotActivityRecord>> ListActivitiesByLessonIdAsync(
        int lessonId, CancellationToken ct = default)
    {
        var rows = await db.SchoolBotActivities.AsNoTracking()
            .Where(r => r.LessonId == lessonId && !r.IsDelete)
            .OrderBy(r => r.Ordinal)
            .ToArrayAsync(ct);
        return rows.Select(From).ToArray();
    }

    public async Task<IReadOnlyList<SchoolBotAuditEventRecord>> ListAuditEventsByEntityAsync(
        string entityType, int entityId, CancellationToken ct = default)
    {
        var rows = await db.SchoolBotAuditEvents.AsNoTracking()
            .Where(r => r.EntityType.ToLower() == entityType.ToLower() && r.EntityId == entityId && !r.IsDelete)
            .OrderBy(r => r.CreateDate)
            .ToArrayAsync(ct);
        return rows.Select(From).ToArray();
    }

    public Task<SchoolBotCurriculumRecord> CreateCurriculumAsync(
        SchoolBotCurriculumRecord curriculum, SchoolBotAuditEventRecord initialAudit, CancellationToken ct = default) =>
        unitOfWork.WriteAsync(async () =>
        {
            if (await db.SchoolBotCurricula.AnyAsync(r => r.CurriculumKey == curriculum.CurriculumKey &&
                    r.Revision == curriculum.Revision && !r.IsDelete, ct))
                throw new DomainRuleException(ErrorCodes.StateConflict, "A curriculum with this key and revision already exists.");
            var now = unitOfWork.Now();
            var row = To(curriculum with
            {
                SchoolBotCurriculumId = 0, CreateDate = now, UpdateDate = now
            });
            db.SchoolBotCurricula.Add(row);
            await db.SaveChangesAsync(ct);
            await unitOfWork.AddAuditAsync(initialAudit, row.SchoolBotCurriculumId, ct);
            return From(row);
        }, ct);

    public Task<SchoolBotCurriculumRecord> UpdateCurriculumMetadataAsync(
        int curriculumId, int levelId, string title, string sourceRef, IReadOnlyList<string> recommendedGrades,
        IReadOnlyList<CurriculumSubject> subjects, string expectedETag, SchoolBotAuditEventRecord auditEvent,
        CancellationToken ct = default) => unitOfWork.WriteAsync(async () =>
        {
            var row = await db.SchoolBotCurricula.SingleOrDefaultAsync(r => r.SchoolBotCurriculumId == curriculumId && !r.IsDelete, ct)
                ?? throw SchoolBotUnitOfWork.Missing("Curriculum");
            var stored = From(row);
            CurriculumService.ValidateStoredMetadata(stored, expectedETag);
            row.LevelId = levelId;
            row.Title = title;
            row.SourceRef = sourceRef;
            row.RecommendedGrades = JsonSerializer.Serialize(recommendedGrades);
            row.Subjects = JsonSerializer.Serialize(subjects);
            row.UpdateDate = unitOfWork.Now(stored.UpdateDate).UtcDateTime;
            row.UpdateBy = auditEvent.CreateBy;
            await unitOfWork.AddAuditAsync(auditEvent, curriculumId, ct);
            return From(row);
        }, ct);

    public Task<SchoolBotCurriculumRecord> PublishCurriculumAsync(
        int curriculumId, string approvalRef, string expectedETag, SchoolBotAuditEventRecord publishAudit,
        CancellationToken ct = default) => unitOfWork.WriteAsync(async () =>
        {
            var row = await db.SchoolBotCurricula.SingleOrDefaultAsync(r => r.SchoolBotCurriculumId == curriculumId && !r.IsDelete, ct)
                ?? throw SchoolBotUnitOfWork.Missing("Curriculum");
            var stored = From(row);
            var audits = await ListAuditEventsByEntityAsync(publishAudit.EntityType, curriculumId, ct);
            if (CurriculumService.IsStoredPublicationReplay(stored, audits, approvalRef, expectedETag, curriculumId)) return stored;
            var now = unitOfWork.Now(stored.UpdateDate);
            row.Lifecycle = "published";
            row.PublishedAt = now.UtcDateTime;
            row.UpdateDate = now.UtcDateTime;
            row.UpdateBy = publishAudit.CreateBy;
            await unitOfWork.AddAuditAsync(publishAudit with { Reason = approvalRef }, curriculumId, ct);
            return From(row);
        }, ct);


    public Task<SchoolBotCurriculumRecord> UpdateCurriculumContentAsync(
        int curriculumId, IReadOnlyList<SchoolBotLessonRecord> newLessons, IReadOnlyList<SchoolBotMissionRecord> newMissions,
        IReadOnlyList<SchoolBotCriteriaRecord> newCriteria, IReadOnlyList<SchoolBotActivityRecord> newActivities,
        string expectedETag, SchoolBotAuditEventRecord auditEvent, CancellationToken ct = default) =>
        unitOfWork.WriteAsync(async () =>
        {
            var curriculum = await db.SchoolBotCurricula.SingleOrDefaultAsync(r => r.SchoolBotCurriculumId == curriculumId && !r.IsDelete, ct)
                ?? throw SchoolBotUnitOfWork.Missing("Curriculum");
            var stored = From(curriculum);
            CurriculumService.ValidateStoredContent(stored, expectedETag);
            if (newLessons.Any(row => row.ImportSource is not null))
                throw new DomainRuleException(ErrorCodes.StateConflict, "ImportSource is not present in the deployed schema.");
            var lessons = await db.SchoolBotLessons.Where(r => r.CurriculumId == curriculumId)
            .ToArrayAsync(ct);
            var lessonIds = lessons.Select(r => r.SchoolBotLessonId).ToArray();
            var missions = await db.SchoolBotMissions.Where(r => lessonIds.Contains(r.LessonId))
            .ToArrayAsync(ct);
            var missionIds = missions.Select(r => r.SchoolBotMissionId).ToArray();
            db.SchoolBotCriteria.RemoveRange(await db.SchoolBotCriteria.Where(r => missionIds.Contains(r.MissionId)).ToArrayAsync(ct));
            db.SchoolBotActivities.RemoveRange(await db.SchoolBotActivities.Where(r => lessonIds.Contains(r.LessonId)).ToArrayAsync(ct));
            db.SchoolBotMissions.RemoveRange(missions);
            db.SchoolBotLessons.RemoveRange(lessons);
            await db.SaveChangesAsync(ct);

            var now = unitOfWork.Now(stored.UpdateDate);
            var lessonMap = newLessons.Select(record => (Record: record, Entity: To(record with
            {
                SchoolBotLessonId = 0, CurriculumId = curriculumId, SchoolId = curriculum.SchoolId, CreateDate = now, UpdateDate = now
            }))).ToArray();
            db.SchoolBotLessons.AddRange(lessonMap.Select(pair => pair.Entity));
            await db.SaveChangesAsync(ct);
            var lessonsById = lessonMap.ToDictionary(pair => pair.Record.SchoolBotLessonId, pair => pair.Entity.SchoolBotLessonId);
            var missionMap = newMissions.Select(record => (Record: record, Entity: To(record with
            {
                SchoolBotMissionId = 0, LessonId = Resolve(lessonsById, record.LessonId), SchoolId = curriculum.SchoolId,
                CreateDate = now, UpdateDate = now
            }))).ToArray();
            db.SchoolBotMissions.AddRange(missionMap.Select(pair => pair.Entity));
            await db.SaveChangesAsync(ct);
            var missionsById = missionMap.ToDictionary(pair => pair.Record.SchoolBotMissionId, pair => pair.Entity.SchoolBotMissionId);
            db.SchoolBotCriteria.AddRange(newCriteria.Select(record => To(record with
            {
                SchoolBotCriteriaId = 0, MissionId = Resolve(missionsById, record.MissionId), SchoolId = curriculum.SchoolId,
                CreateDate = now, UpdateDate = now
            })));
            db.SchoolBotActivities.AddRange(newActivities.Select(record => To(record with
            {
                SchoolBotActivityId = 0, LessonId = Resolve(lessonsById, record.LessonId), SchoolId = curriculum.SchoolId,
                CreateDate = now, UpdateDate = now
            })));
            curriculum.UpdateDate = now.UtcDateTime;
            curriculum.UpdateBy = auditEvent.CreateBy;
            await unitOfWork.AddAuditAsync(auditEvent, curriculumId, ct);
            return From(curriculum);
        }, ct);

    private static int Resolve(IReadOnlyDictionary<int, int> map, int id) => map.TryGetValue(id, out var value)
        ? value : throw new DomainRuleException(ErrorCodes.InvalidInput, "Content references a missing parent.");
    public Task<IdempotentCommitResult<T>> CommitWithIdempotencyAsync<T>(IdempotencyCommand command,
        Func<ISchoolBotRepositoryScope, int?, DateTimeOffset, CancellationToken, Task> recheckAccess,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<MutationResult<T>>> mutate, CancellationToken ct = default) =>
        unitOfWork.CommitWithIdempotencyAsync(command, recheckAccess, mutate, ct);

}
