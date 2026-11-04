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
    IReadOnlyList<SchoolBotStaffRoleRecord>? StaffRoles = null)
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
