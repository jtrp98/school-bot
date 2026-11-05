using System.Globalization;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using ServiceSchoolBot.Domain.Curriculum;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.Tests;

// Converts between the curriculum request/response records the tests assert on and the CurriculumService
// messages, the same way the Gateway translates JSON bodies.
internal static class CurriculumRpc
{
    public static string Time(DateTimeOffset value) => value.ToString("O", CultureInfo.InvariantCulture);

    public static DateTimeOffset ParseTime(string value) =>
        DateTimeOffset.ParseExact(value, "O", CultureInfo.InvariantCulture);

    private static DateTimeOffset? OptionalTime(bool present, string value) => present ? ParseTime(value) : null;

    public static Rpc.CurriculumSubjectItem ToRpc(CurriculumSubject subject) => new()
    {
        SubjectKey = subject.SubjectKey.ToString("D"),
        Code = subject.Code,
        Title = subject.Title
    };

    public static Rpc.LessonItem ToRpc(LessonInput lesson)
    {
        var result = new Rpc.LessonItem
        {
            LessonKey = lesson.LessonKey.ToString("D"),
            Ordinal = lesson.Ordinal,
            Title = lesson.Title,
            Objective = lesson.Objective,
            Instructions = lesson.Instructions
        };
        result.Missions.AddRange(lesson.Missions.Select(ToRpc));
        result.Activities.AddRange(lesson.Activities.Select(ToRpc));
        return result;
    }

    private static Rpc.MissionItem ToRpc(MissionInput mission)
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

    private static Rpc.ActivityItem ToRpc(ActivityInput activity)
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

    public static Rpc.CreateCurriculumRequest Create(CreateCurriculumRequest body, Guid? idempotencyKey = null)
    {
        var request = new Rpc.CreateCurriculumRequest
        {
            CurriculumKey = body.CurriculumKey.ToString("D"),
            Revision = body.Revision,
            LevelId = body.LevelId,
            Title = body.Title,
            SourceRef = body.SourceRef
        };
        request.RecommendedGrades.AddRange(body.RecommendedGrades);
        request.Subjects.AddRange(body.Subjects.Select(ToRpc));
        if (idempotencyKey is { } key) request.IdempotencyKey = key.ToString("D");
        return request;
    }

    public static Rpc.UpdateCurriculumMetadataRequest Metadata(int id, CurriculumMetadataInput body, string? ifMatch,
        Guid? idempotencyKey = null)
    {
        var request = new Rpc.UpdateCurriculumMetadataRequest
        {
            Id = id,
            LevelId = body.LevelId,
            Title = body.Title,
            SourceRef = body.SourceRef
        };
        request.RecommendedGrades.AddRange(body.RecommendedGrades);
        request.Subjects.AddRange(body.Subjects.Select(ToRpc));
        if (ifMatch is not null) request.IfMatch = ifMatch;
        if (idempotencyKey is { } key) request.IdempotencyKey = key.ToString("D");
        return request;
    }

    public static Rpc.UpdateCurriculumContentRequest Content(int id, UpdateCurriculumContentRequest body, string? ifMatch,
        Guid? idempotencyKey = null)
    {
        var request = new Rpc.UpdateCurriculumContentRequest { Id = id };
        request.Lessons.AddRange(body.Lessons.Select(ToRpc));
        if (ifMatch is not null) request.IfMatch = ifMatch;
        if (idempotencyKey is { } key) request.IdempotencyKey = key.ToString("D");
        return request;
    }

    public static Rpc.PublishCurriculumRequest Publish(int id, string? approvalRef, string? ifMatch,
        Guid? idempotencyKey = null)
    {
        var request = new Rpc.PublishCurriculumRequest { Id = id };
        if (approvalRef is not null) request.ApprovalRef = approvalRef;
        if (ifMatch is not null) request.IfMatch = ifMatch;
        if (idempotencyKey is { } key) request.IdempotencyKey = key.ToString("D");
        return request;
    }

    public static Rpc.ImportCurriculumRequest Import(int id, CurriculumImportRequest body, Guid? idempotencyKey)
    {
        var request = new Rpc.ImportCurriculumRequest { Id = id, SourceRef = body.SourceRef };
        request.Rows.AddRange(body.Rows.Select(row =>
        {
            var item = new Rpc.CurriculumImportRowItem
            {
                SourceRowKey = row.SourceRowKey.ToString("D"),
                SourcePosition = row.SourcePosition,
                Lesson = ToRpc(row.Lesson)
            };
            if (row.SourceApproved is { } approved) item.SourceApproved = approved;
            return item;
        }));
        if (idempotencyKey is { } key) request.IdempotencyKey = key.ToString("D");
        return request;
    }

    public static CurriculumMutationResponse ToModel(Rpc.CurriculumMutationResponse response) =>
        new(response.Id, ParseTime(response.UpdateDate), OptionalTime(response.HasPublishedAt, response.PublishedAt));

    public static CurriculumImportResponse ToModel(Rpc.CurriculumImportResponse response) =>
        new(response.Id, ParseTime(response.UpdateDate), response.Lessons
            .Select(lesson => new CurriculumImportLessonResult(Guid.Parse(lesson.SourceRowKey), lesson.LessonId,
                lesson.Outcome)).ToList());

    public static CurriculumSummary ToModel(Rpc.CurriculumSummaryItem item) =>
        new(item.Id, Guid.Parse(item.CurriculumKey), item.Revision, item.LevelId, item.Title, item.Lifecycle,
            item.SourceRef, OptionalTime(item.HasPublishedAt, item.PublishedAt), ParseTime(item.UpdateDate));

    public static CurriculumListResponse ToModel(Rpc.CurriculumListResponse response) =>
        new(response.Items.Select(ToModel).ToList(), response.HasNextCursor ? response.NextCursor : null);

    private static CurriculumApprovalView? ToModel(Rpc.CurriculumApprovalItem? approval) => approval is null
        ? null
        : new CurriculumApprovalView(approval.Actor, ParseTime(approval.ApprovedAt), approval.ApprovalRef);

    private static CurriculumSubject ToModel(Rpc.CurriculumSubjectItem subject) =>
        new(Guid.Parse(subject.SubjectKey), subject.Code, subject.Title);

    public static LessonInput ToModel(Rpc.LessonItem lesson) => new(
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
                OptionalTime(mission.Criteria.HasApprovedAt, mission.Criteria.ApprovedAt)))).ToList(),
        lesson.Activities.Select(activity => new ActivityInput(
            Guid.Parse(activity.ActivityKey),
            activity.Ordinal,
            activity.Title,
            activity.Type,
            activity.HasMode ? activity.Mode : null,
            activity.HasIndicators ? activity.Indicators : null,
            activity.HasConfig ? activity.Config : null)).ToList());

    public static CurriculumDraftView ToModel(Rpc.CurriculumDraftResponse response)
    {
        var summary = response.Summary;
        return new CurriculumDraftView(summary.Id, Guid.Parse(summary.CurriculumKey), summary.Revision, summary.LevelId,
            summary.Title, summary.Lifecycle, summary.SourceRef, OptionalTime(summary.HasPublishedAt, summary.PublishedAt),
            ParseTime(summary.UpdateDate), response.RecommendedGrades.ToList(), response.Subjects.Select(ToModel).ToList(),
            response.Lessons.Select(ToModel).ToList(), ToModel(response.Approval));
    }

    public static CurriculumAdminView ToModel(Rpc.CurriculumAdminResponse response)
    {
        var summary = response.Summary;
        return new CurriculumAdminView(summary.Id, Guid.Parse(summary.CurriculumKey), summary.Revision, summary.LevelId,
            summary.Title, summary.Lifecycle, summary.SourceRef, OptionalTime(summary.HasPublishedAt, summary.PublishedAt),
            ParseTime(summary.UpdateDate), response.RecommendedGrades.ToList(), response.Subjects.Select(ToModel).ToList(),
            response.Lessons.Select(admin =>
            {
                var lesson = ToModel(admin.Lesson);
                var source = admin.ImportSource is { } item
                    ? new LessonImportSource(item.SourceRef, item.SourceRowKey,
                        item.HasSourceApproved ? item.SourceApproved : null, item.RowHash)
                    : null;
                return new AdminLessonView(lesson.LessonKey, lesson.Ordinal, lesson.Title, lesson.Objective,
                    lesson.Instructions, lesson.Missions, lesson.Activities, source);
            }).ToList(), ToModel(response.Approval));
    }
}
