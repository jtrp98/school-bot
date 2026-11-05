using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface ICurriculumRepository
{
    Task<SchoolBotCurriculumRecord?> GetCurriculumByIdAsync(int curriculumId, CancellationToken ct = default);
    Task<SchoolBotCurriculumRecord?> GetCurriculumByKeyAndRevisionAsync(Guid curriculumKey, int revision, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotCurriculumRecord>> ListCurriculaAsync(int? levelId = null, string? lifecycle = null, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotLessonRecord>> ListLessonsByCurriculumIdAsync(int curriculumId, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotMissionRecord>> ListMissionsByLessonIdAsync(int lessonId, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotCriteriaRecord>> ListCriteriaByMissionIdAsync(int missionId, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotActivityRecord>> ListActivitiesByLessonIdAsync(int lessonId, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotAuditEventRecord>> ListAuditEventsByEntityAsync(string entityType, int entityId, CancellationToken ct = default);
    Task<SchoolBotCurriculumRecord> CreateCurriculumAsync(
        SchoolBotCurriculumRecord curriculum,
        SchoolBotAuditEventRecord initialAudit,
        CancellationToken ct = default);
    Task<SchoolBotCurriculumRecord> UpdateCurriculumMetadataAsync(
        int curriculumId,
        int levelId,
        string title,
        string sourceRef,
        IReadOnlyList<string> recommendedGrades,
        IReadOnlyList<CurriculumSubject> subjects,
        string expectedETag,
        SchoolBotAuditEventRecord auditEvent,
        CancellationToken ct = default);
    Task<SchoolBotCurriculumRecord> UpdateCurriculumContentAsync(
        int curriculumId,
        IReadOnlyList<SchoolBotLessonRecord> newLessons,
        IReadOnlyList<SchoolBotMissionRecord> newMissions,
        IReadOnlyList<SchoolBotCriteriaRecord> newCriteria,
        IReadOnlyList<SchoolBotActivityRecord> newActivities,
        string expectedETag,
        SchoolBotAuditEventRecord auditEvent,
        CancellationToken ct = default);
    Task<SchoolBotCurriculumRecord> PublishCurriculumAsync(
        int curriculumId,
        string approvalRef,
        string expectedETag,
        SchoolBotAuditEventRecord publishAudit,
        CancellationToken ct = default);
}
