using System.Text.Json.Serialization;

namespace ServiceSchoolBot.Domain.Curriculum;

public sealed record CurriculumSubject(
    Guid SubjectKey,
    string Code,
    string Title
);

public sealed record SchoolBotCurriculumRecord(
    int SchoolBotCurriculumId,
    Guid CurriculumKey,
    int Revision,
    int LevelId,
    string Title,
    IReadOnlyList<string> RecommendedGrades,
    IReadOnlyList<CurriculumSubject> Subjects,
    string SourceRef,
    string Lifecycle,
    DateTimeOffset? PublishedAt,
    bool IsDelete,
    int? CreateBy,
    DateTimeOffset? CreateDate,
    int? UpdateBy,
    DateTimeOffset? UpdateDate,
    int? SchoolId = null
);

public sealed record SchoolBotLessonRecord(
    int SchoolBotLessonId,
    int CurriculumId,
    Guid LessonKey,
    int Ordinal,
    string Title,
    string Objective,
    string Instructions,
    bool IsDelete,
    int? CreateBy,
    DateTimeOffset? CreateDate,
    int? UpdateBy,
    DateTimeOffset? UpdateDate,
    int? SchoolId = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ImportSource = null
);

public sealed record SchoolBotMissionRecord(
    int SchoolBotMissionId,
    int LessonId,
    Guid MissionKey,
    int Ordinal,
    string Title,
    string Instructions,
    string Method,
    bool IsDelete,
    int? CreateBy,
    DateTimeOffset? CreateDate,
    int? UpdateBy,
    DateTimeOffset? UpdateDate,
    int? SchoolId = null
);

public sealed record SchoolBotCriteriaRecord(
    int SchoolBotCriteriaId,
    int MissionId,
    int Revision,
    string Rules,
    string SourceRef,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    bool IsDelete,
    int? CreateBy,
    DateTimeOffset? CreateDate,
    int? UpdateBy,
    DateTimeOffset? UpdateDate,
    int? SchoolId = null
);

public sealed record SchoolBotActivityRecord(
    int SchoolBotActivityId,
    int LessonId,
    Guid ActivityKey,
    int Ordinal,
    string Title,
    string Type,
    string? Mode,
    string Indicators,
    string Config,
    bool IsDelete,
    int? CreateBy,
    DateTimeOffset? CreateDate,
    int? UpdateBy,
    DateTimeOffset? UpdateDate,
    int? SchoolId = null
);

public sealed record SchoolBotAuditEventRecord(
    int SchoolBotAuditEventId,
    string Actor,
    string Event,
    string EntityType,
    int EntityId,
    Guid RequestId,
    string Reason,
    bool IsDelete,
    int? CreateBy,
    DateTimeOffset? CreateDate,
    int? UpdateBy,
    DateTimeOffset? UpdateDate,
    int? SchoolId = null
);
