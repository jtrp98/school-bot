using System.Text.Json.Serialization;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Identity;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Domain.Assessment;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage;

public sealed record CurriculumIdempotencyRecord(
    int SchoolBotIdempotencyId,
    string Principal,
    string Operation,
    Guid RequestKey,
    string RequestHash,
    string? NonceHash,
    int HttpStatus,
    byte[]? ResultCipher,
    string? ResultKeyId,
    int? ResourceId,
    DateTimeOffset CompletedAt,
    DateTimeOffset ReplayUntil,
    bool IsDelete,
    int? CreateBy,
    DateTimeOffset? CreateDate,
    int? UpdateBy,
    DateTimeOffset? UpdateDate,
    int? SchoolId);

public sealed record CurriculumStoreSnapshot(
    [property: JsonPropertyName("format")] int Format,
    [property: JsonPropertyName("curricula")] IReadOnlyList<SchoolBotCurriculumRecord> Curricula,
    [property: JsonPropertyName("lessons")] IReadOnlyList<SchoolBotLessonRecord> Lessons,
    [property: JsonPropertyName("missions")] IReadOnlyList<SchoolBotMissionRecord> Missions,
    [property: JsonPropertyName("criteria")] IReadOnlyList<SchoolBotCriteriaRecord> Criteria,
    [property: JsonPropertyName("activities")] IReadOnlyList<SchoolBotActivityRecord> Activities,
    [property: JsonPropertyName("audit_events")] IReadOnlyList<SchoolBotAuditEventRecord> AuditEvents,
    [property: JsonPropertyName("idempotency")] IReadOnlyList<CurriculumIdempotencyRecord>? Idempotency = null,
    [property: JsonPropertyName("question_banks"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<SchoolBotQuestionBankRecord>? QuestionBanks = null,
    [property: JsonPropertyName("course_runs"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<SchoolBotCourseRunRecord>? CourseRuns = null,
    [property: JsonPropertyName("course_run_activities"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<SchoolBotCourseRunActivityRecord>? CourseRunActivities = null,
    [property: JsonPropertyName("attempts"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<SchoolBotAttemptRecord>? Attempts = null,
    [property: JsonPropertyName("sessions"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<SchoolBotSessionRecord>? Sessions = null,
    [property: JsonPropertyName("staff_roles"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<SchoolBotStaffRoleRecord>? StaffRoles = null,
    [property: JsonPropertyName("submissions"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<SchoolBotSubmissionRecord>? Submissions = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<GradeSlotRecord>? GradeSlots = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<ScoreMappingRecord>? ScoreMappings = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<ScorePreviewRecord>? ScorePreviews = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<ScoreExportRecord>? ScoreExports = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<ScoreOutboxRecord>? ScoreOutbox = null,
    [property: JsonPropertyName("assessments"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<SchoolBotAssessmentRecord>? Assessments = null,
    [property: JsonPropertyName("grading_jobs"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<SchoolBotGradingJobRecord>? GradingJobs = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<SchoolBotRetakeGrantRecord>? RetakeGrants = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<SchoolBotScoringPolicyRecord>? ScoringPolicies = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<SchoolBotLevelRecord>? Levels = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<SchoolBotLevelPromotionPolicyRecord>? PromotionPolicies = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<SchoolBotLevelProposalRecord>? LevelProposals = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<ServiceSchoolBot.Domain.Releases.DesktopRelease>? Releases = null)
{
    public static CurriculumStoreSnapshot Empty => new(1, [], [], [], [], [], [], []);
}

public sealed record CurriculumStoreMutation<T>(CurriculumStoreSnapshot Snapshot, T Result, bool Changed = true);

public interface IWebSessionBackingStore
{
    Task<T> MutateWebSessionUnitAsync<T>(
        Func<CurriculumStoreSnapshot, IReadOnlyList<SchoolBotLicenseRecord>, CurriculumStoreMutation<T>> mutate,
        CancellationToken ct);
}

public interface ICurriculumBackingStore
{
    Task<CurriculumStoreSnapshot> LoadCurriculumUnitAsync(CancellationToken ct = default);
    Task<T> MutateCurriculumUnitAsync<T>(
        Func<CurriculumStoreSnapshot, CancellationToken, Task<CurriculumStoreMutation<T>>> mutate,
        CancellationToken ct = default);
}
