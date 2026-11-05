using System.Globalization;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.API.GrpcServices;

internal static class CurriculumProtoMapper
{
    public static string Time(DateTimeOffset value) => value.ToString("O", CultureInfo.InvariantCulture);

    public static DateTimeOffset ParseTime(string value) =>
        DateTimeOffset.ParseExact(value, "O", CultureInfo.InvariantCulture);

    public static CurriculumSubject ToDomain(Rpc.CurriculumSubjectItem subject) =>
        new(Guid.Parse(subject.SubjectKey), subject.Code, subject.Title);

    public static LessonInput ToDomain(Rpc.LessonItem lesson) => new(
        Guid.Parse(lesson.LessonKey),
        lesson.Ordinal,
        lesson.Title,
        lesson.Objective,
        lesson.Instructions,
        lesson.Missions.Select(mission => new MissionInput(
            Guid.Parse(mission.MissionKey),
            mission.Ordinal,
            mission.Title,
            mission.Instructions,
            mission.Method,
            new CriteriaInput(
                mission.Criteria.Revision,
                mission.Criteria.Rules,
                mission.Criteria.SourceRef,
                mission.Criteria.HasApprovedBy ? mission.Criteria.ApprovedBy : null,
                mission.Criteria.HasApprovedAt ? ParseTime(mission.Criteria.ApprovedAt) : null))).ToList(),
        lesson.Activities.Select(activity => new ActivityInput(
            Guid.Parse(activity.ActivityKey),
            activity.Ordinal,
            activity.Title,
            activity.Type,
            activity.HasMode ? activity.Mode : null,
            activity.HasIndicators ? activity.Indicators : null,
            activity.HasConfig ? activity.Config : null)).ToList());

    public static Rpc.CurriculumSubjectItem ToProto(CurriculumSubject subject) => new()
    {
        SubjectKey = subject.SubjectKey.ToString("D"),
        Code = subject.Code,
        Title = subject.Title
    };

    public static Rpc.LessonItem ToProto(LessonInput lesson)
    {
        var result = new Rpc.LessonItem
        {
            LessonKey = lesson.LessonKey.ToString("D"),
            Ordinal = lesson.Ordinal,
            Title = lesson.Title,
            Objective = lesson.Objective,
            Instructions = lesson.Instructions
        };
        result.Missions.AddRange(lesson.Missions.Select(ToProto));
        result.Activities.AddRange(lesson.Activities.Select(ToProto));
        return result;
    }

    public static Rpc.LessonItem ToProto(AdminLessonView lesson) => ToProto(new LessonInput(lesson.LessonKey,
        lesson.Ordinal, lesson.Title, lesson.Objective, lesson.Instructions, lesson.Missions, lesson.Activities));

    private static Rpc.MissionItem ToProto(MissionInput mission)
    {
        var criteria = new Rpc.CriteriaItem
        {
            Revision = mission.Criteria.Revision,
            Rules = mission.Criteria.Rules,
            SourceRef = mission.Criteria.SourceRef
        };
        if (mission.Criteria.ApprovedBy is not null) criteria.ApprovedBy = mission.Criteria.ApprovedBy;
        if (mission.Criteria.ApprovedAt is { } approvedAt) criteria.ApprovedAt = Time(approvedAt);
        return new Rpc.MissionItem
        {
            MissionKey = mission.MissionKey.ToString("D"),
            Ordinal = mission.Ordinal,
            Title = mission.Title,
            Instructions = mission.Instructions,
            Method = mission.Method,
            Criteria = criteria
        };
    }

    private static Rpc.ActivityItem ToProto(ActivityInput activity)
    {
        var result = new Rpc.ActivityItem
        {
            ActivityKey = activity.ActivityKey.ToString("D"),
            Ordinal = activity.Ordinal,
            Title = activity.Title,
            Type = activity.Type
        };
        if (activity.Mode is not null) result.Mode = activity.Mode;
        if (activity.Indicators is not null) result.Indicators = activity.Indicators;
        if (activity.Config is not null) result.Config = activity.Config;
        return result;
    }

    public static Rpc.CurriculumSummaryItem Summary(int id, Guid key, int revision, int levelId, string title,
        string lifecycle, string sourceRef, DateTimeOffset? publishedAt, DateTimeOffset updateDate)
    {
        var result = new Rpc.CurriculumSummaryItem
        {
            Id = id,
            CurriculumKey = key.ToString("D"),
            Revision = revision,
            LevelId = levelId,
            Title = title,
            Lifecycle = lifecycle,
            SourceRef = sourceRef,
            UpdateDate = Time(updateDate)
        };
        if (publishedAt is { } published) result.PublishedAt = Time(published);
        return result;
    }

    public static Rpc.CurriculumListResponse ToProto(CurriculumListResponse list)
    {
        var result = new Rpc.CurriculumListResponse();
        if (list.NextCursor is not null) result.NextCursor = list.NextCursor;
        result.Items.AddRange(list.Items.Select(item => Summary(item.Id, item.CurriculumKey, item.Revision,
            item.LevelId, item.Title, item.Lifecycle, item.SourceRef, item.PublishedAt, item.UpdateDate)));
        return result;
    }

    public static Rpc.CurriculumApprovalItem? ToProto(CurriculumApprovalView? approval) => approval is null
        ? null
        : new Rpc.CurriculumApprovalItem
        {
            Actor = approval.Actor,
            ApprovedAt = Time(approval.ApprovedAt),
            ApprovalRef = approval.ApprovalRef
        };

    public static Rpc.CurriculumDraftResponse ToProto(CurriculumDraftView view)
    {
        var result = new Rpc.CurriculumDraftResponse
        {
            Summary = Summary(view.Id, view.CurriculumKey, view.Revision, view.LevelId, view.Title, view.Lifecycle,
                view.SourceRef, view.PublishedAt, view.UpdateDate),
            Approval = ToProto(view.Approval),
            Etag = OpaqueConcurrencyToken.ToETag(view.UpdateDate)
        };
        result.RecommendedGrades.AddRange(view.RecommendedGrades);
        result.Subjects.AddRange(view.Subjects.Select(ToProto));
        result.Lessons.AddRange(view.Lessons.Select(ToProto));
        return result;
    }

    public static Rpc.CurriculumAdminResponse ToProto(CurriculumAdminView view)
    {
        var result = new Rpc.CurriculumAdminResponse
        {
            Summary = Summary(view.Id, view.CurriculumKey, view.Revision, view.LevelId, view.Title, view.Lifecycle,
                view.SourceRef, view.PublishedAt, view.UpdateDate),
            Approval = ToProto(view.Approval),
            Etag = OpaqueConcurrencyToken.ToETag(view.UpdateDate)
        };
        result.RecommendedGrades.AddRange(view.RecommendedGrades);
        result.Subjects.AddRange(view.Subjects.Select(ToProto));
        result.Lessons.AddRange(view.Lessons.Select(lesson =>
        {
            var admin = new Rpc.AdminLessonItem { Lesson = ToProto(lesson) };
            if (lesson.ImportSource is { } source) admin.ImportSource = ToProto(source);
            return admin;
        }));
        return result;
    }

    private static Rpc.LessonImportSourceItem ToProto(LessonImportSource source)
    {
        var result = new Rpc.LessonImportSourceItem
        {
            SourceRef = source.SourceRef,
            SourceRowKey = source.SourceRowKey,
            RowHash = source.RowHash
        };
        if (source.SourceApproved is { } approved) result.SourceApproved = approved;
        return result;
    }

    public static Rpc.CurriculumMutationResponse ToProto(CurriculumMutationResponse mutation)
    {
        var result = new Rpc.CurriculumMutationResponse
        {
            Id = mutation.Id,
            UpdateDate = Time(mutation.UpdateDate),
            Etag = OpaqueConcurrencyToken.ToETag(mutation.UpdateDate)
        };
        if (mutation.PublishedAt is { } published) result.PublishedAt = Time(published);
        return result;
    }

    public static Rpc.CurriculumImportResponse ToProto(CurriculumImportResponse imported)
    {
        var result = new Rpc.CurriculumImportResponse
        {
            Id = imported.Id,
            UpdateDate = Time(imported.UpdateDate),
            Etag = OpaqueConcurrencyToken.ToETag(imported.UpdateDate)
        };
        result.Lessons.AddRange(imported.Lessons.Select(lesson => new Rpc.CurriculumImportLessonResultItem
        {
            SourceRowKey = lesson.SourceRowKey.ToString("D"),
            LessonId = lesson.LessonId,
            Outcome = lesson.Outcome
        }));
        return result;
    }
}
