using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed partial class CurriculumRepository
{
    private static SchoolBotCurriculumRecord From(SchoolBotCurriculum row) => new(
        row.SchoolBotCurriculumId,
        row.CurriculumKey,
        row.Revision,
        row.LevelId,
        row.Title,
        JsonSerializer.Deserialize<IReadOnlyList<string>>(row.RecommendedGrades) ?? throw new InvalidOperationException("Invalid stored RecommendedGrades."),
        JsonSerializer.Deserialize<IReadOnlyList<CurriculumSubject>>(row.Subjects) ?? throw new InvalidOperationException("Invalid stored Subjects."),
        row.SourceRef,
        row.Lifecycle,
        Offset(row.PublishedAt),
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotCurriculum To(SchoolBotCurriculumRecord row) => new()
    {
        SchoolBotCurriculumId = row.SchoolBotCurriculumId,
        CurriculumKey = row.CurriculumKey,
        Revision = row.Revision,
        LevelId = row.LevelId,
        Title = row.Title,
        RecommendedGrades = JsonSerializer.Serialize(row.RecommendedGrades),
        Subjects = JsonSerializer.Serialize(row.Subjects),
        SourceRef = row.SourceRef,
        Lifecycle = row.Lifecycle,
        PublishedAt = row.PublishedAt?.UtcDateTime,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static SchoolBotLessonRecord From(SchoolBotLesson row) => new(
        row.SchoolBotLessonId,
        row.CurriculumId,
        row.LessonKey,
        row.Ordinal,
        row.Title,
        row.Objective,
        row.Instructions,
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId,
        null);

    private static SchoolBotLesson To(SchoolBotLessonRecord row) => new()
    {
        SchoolBotLessonId = row.SchoolBotLessonId,
        CurriculumId = row.CurriculumId,
        LessonKey = row.LessonKey,
        Ordinal = row.Ordinal,
        Title = row.Title,
        Objective = row.Objective,
        Instructions = row.Instructions,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static SchoolBotMissionRecord From(SchoolBotMission row) => new(
        row.SchoolBotMissionId,
        row.LessonId,
        row.MissionKey,
        row.Ordinal,
        row.Title,
        row.Instructions,
        row.Method,
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotMission To(SchoolBotMissionRecord row) => new()
    {
        SchoolBotMissionId = row.SchoolBotMissionId,
        LessonId = row.LessonId,
        MissionKey = row.MissionKey,
        Ordinal = row.Ordinal,
        Title = row.Title,
        Instructions = row.Instructions,
        Method = row.Method,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static SchoolBotCriteriaRecord From(SchoolBotCriterion row) => new(
        row.SchoolBotCriteriaId,
        row.MissionId,
        row.Revision,
        row.Rules,
        row.SourceRef,
        row.ApprovedBy,
        Offset(row.ApprovedAt),
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotCriterion To(SchoolBotCriteriaRecord row) => new()
    {
        SchoolBotCriteriaId = row.SchoolBotCriteriaId,
        MissionId = row.MissionId,
        Revision = row.Revision,
        Rules = row.Rules,
        SourceRef = row.SourceRef,
        ApprovedBy = row.ApprovedBy,
        ApprovedAt = row.ApprovedAt?.UtcDateTime,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static SchoolBotActivityRecord From(SchoolBotActivity row) => new(
        row.SchoolBotActivityId,
        row.LessonId,
        row.ActivityKey,
        row.Ordinal,
        row.Title,
        row.Type,
        row.Mode,
        row.Indicators,
        row.Config,
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotActivity To(SchoolBotActivityRecord row) => new()
    {
        SchoolBotActivityId = row.SchoolBotActivityId,
        LessonId = row.LessonId,
        ActivityKey = row.ActivityKey,
        Ordinal = row.Ordinal,
        Title = row.Title,
        Type = row.Type,
        Mode = row.Mode,
        Indicators = row.Indicators,
        Config = row.Config,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static SchoolBotAuditEventRecord From(SchoolBotAuditEvent row) => new(
        row.SchoolBotAuditEventId,
        row.Actor,
        row.Event,
        row.EntityType,
        row.EntityId,
        row.RequestId,
        row.Reason,
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotAuditEvent To(SchoolBotAuditEventRecord row) => new()
    {
        SchoolBotAuditEventId = row.SchoolBotAuditEventId,
        Actor = row.Actor,
        Event = row.Event,
        EntityType = row.EntityType,
        EntityId = row.EntityId,
        RequestId = row.RequestId,
        Reason = row.Reason,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static DateTimeOffset Offset(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    private static DateTimeOffset? Offset(DateTime? value) => value is { } date ? Offset(date) : null;
}
