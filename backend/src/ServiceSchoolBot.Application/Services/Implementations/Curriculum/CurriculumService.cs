using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Interfaces.Curriculum;
using System.Security.Cryptography;
using System.Text;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Services.Implementations.Curriculum;

public sealed partial class CurriculumService(
    ICurriculumRepository dbAdapter,
    TimeProvider clock) : ICurriculumService
{
    private const int MaxRefLength = 1000;
    private const int MaxSourceRefLength = 2048;

    private static string ComputeSha256(string input) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(input)));

    public async Task<CurriculumAdminView> GetDraftViewAsync(
        CurriculumPrincipalContext principal,
        int id,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var now = clock.GetUtcNow();
        var curriculum = await dbAdapter.GetCurriculumByIdAsync(id, ct);
        if (curriculum is null)
            throw new DomainRuleException(ErrorCodes.NotFound, "Curriculum not found.");

        var authCheck = CurriculumAuthorizationService.EvaluateReadCurriculum(principal, false, now);
        if (!authCheck.IsAllowed)
            throw new DomainRuleException(authCheck.ErrorCode!, authCheck.Message!);

        var view = await BuildDraftViewAsync(curriculum, ct);
        var storedLessons = await dbAdapter.ListLessonsByCurriculumIdAsync(id, ct);
        var adminLessons = view.Lessons.Select(lesson =>
        {
            var stored = storedLessons.Single(row => row.LessonKey == lesson.LessonKey);
            return new AdminLessonView(lesson.LessonKey, lesson.Ordinal, lesson.Title, lesson.Objective,
                lesson.Instructions, lesson.Missions, lesson.Activities, stored.ImportSource is null ? null
                    : LessonImportSourceCodec.Decode(stored.ImportSource, stored.LessonKey));
        }).ToArray();
        return new CurriculumAdminView(view.Id, view.CurriculumKey, view.Revision, view.LevelId,
            view.Title, view.Lifecycle, view.SourceRef, view.PublishedAt, view.UpdateDate,
            view.RecommendedGrades, view.Subjects, adminLessons, view.Approval);
    }

    public async Task<CurriculumDraftView> GetPublishedViewAsync(
        CurriculumPrincipalContext principal,
        int id,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var now = clock.GetUtcNow();
        var curriculum = await dbAdapter.GetCurriculumByIdAsync(id, ct);
        if (curriculum is null)
            throw new DomainRuleException(ErrorCodes.NotFound, "Curriculum not found.");

        var isPublished = string.Equals(curriculum.Lifecycle, "published", StringComparison.OrdinalIgnoreCase);
        var access = CurriculumAuthorizationService.EvaluateReadCurriculum(principal, isPublished, now);
        if (!access.IsAllowed)
            throw new DomainRuleException(access.ErrorCode!, access.Message!);

        return await BuildDraftViewAsync(curriculum, ct);
    }

    public async Task<CurriculumListResponse> ListPublishedCurriculaAsync(
        CurriculumPrincipalContext principal,
        int? levelId = null,
        string? recommendedGrade = null,
        Guid? subjectKey = null,
        string? mode = null,
        int limit = 50,
        string? cursor = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var now = clock.GetUtcNow();
        var authCheck = CurriculumAuthorizationService.EvaluateReadCurriculum(principal, true, now);
        if (!authCheck.IsAllowed)
            throw new DomainRuleException(authCheck.ErrorCode!, authCheck.Message!);

        var effectiveLimit = Math.Clamp(limit, 1, 100);
        var publishedCurricula = await dbAdapter.ListCurriculaAsync(levelId, "published", ct);

        var filtered = new List<SchoolBotCurriculumRecord>();
        foreach (var c in publishedCurricula)
        {
            if (!string.IsNullOrWhiteSpace(recommendedGrade) &&
                !c.RecommendedGrades.Any(g => string.Equals(g, recommendedGrade, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (subjectKey.HasValue && !c.Subjects.Any(s => s.SubjectKey == subjectKey.Value))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(mode))
            {
                var lessons = await dbAdapter.ListLessonsByCurriculumIdAsync(c.SchoolBotCurriculumId, ct);
                var hasMode = false;
                foreach (var l in lessons)
                {
                    var activities = await dbAdapter.ListActivitiesByLessonIdAsync(l.SchoolBotLessonId, ct);
                    if (activities.Any(a => string.Equals(a.Mode, mode, StringComparison.OrdinalIgnoreCase)))
                    {
                        hasMode = true;
                        break;
                    }
                }
                if (!hasMode) continue;
            }

            filtered.Add(c);
        }

        var offset = 0;
        if (!string.IsNullOrWhiteSpace(cursor) && int.TryParse(cursor, out var parsedOffset))
            offset = Math.Max(0, parsedOffset);

        var pageItems = filtered.Skip(offset).Take(effectiveLimit).Select(ToSummary).ToArray();
        var nextCursor = (offset + pageItems.Length < filtered.Count) ? (offset + pageItems.Length).ToString() : null;

        return new CurriculumListResponse(pageItems, nextCursor);
    }

    public async Task<CurriculumListResponse> ListAdminCurriculaAsync(
        CurriculumPrincipalContext principal,
        string? lifecycle = null,
        int? levelId = null,
        int limit = 50,
        string? cursor = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var now = clock.GetUtcNow();
        var authCheck = CurriculumAuthorizationService.EvaluateReadCurriculum(principal, false, now);
        if (!authCheck.IsAllowed)
            throw new DomainRuleException(authCheck.ErrorCode!, authCheck.Message!);

        var effectiveLimit = Math.Clamp(limit, 1, 100);
        var curricula = await dbAdapter.ListCurriculaAsync(levelId, lifecycle, ct);

        var offset = 0;
        if (!string.IsNullOrWhiteSpace(cursor) && int.TryParse(cursor, out var parsedOffset))
            offset = Math.Max(0, parsedOffset);

        var pageItems = curricula.Skip(offset).Take(effectiveLimit).Select(ToSummary).ToArray();
        var nextCursor = (offset + pageItems.Length < curricula.Count) ? (offset + pageItems.Length).ToString() : null;

        return new CurriculumListResponse(pageItems, nextCursor);
    }

    private async Task<CurriculumDraftView> BuildDraftViewAsync(SchoolBotCurriculumRecord curriculum, CancellationToken ct)
    {
        var rawLessons = await dbAdapter.ListLessonsByCurriculumIdAsync(curriculum.SchoolBotCurriculumId, ct);
        var lessonViews = new List<LessonInput>();

        foreach (var l in rawLessons)
        {
            var rawMissions = await dbAdapter.ListMissionsByLessonIdAsync(l.SchoolBotLessonId, ct);
            var missionViews = new List<MissionInput>();

            foreach (var m in rawMissions)
            {
                var rawCriteria = await dbAdapter.ListCriteriaByMissionIdAsync(m.SchoolBotMissionId, ct);
                var activeCrit = rawCriteria.FirstOrDefault();
                var critView = activeCrit is not null
                    ? new CriteriaInput(activeCrit.Revision, activeCrit.Rules, activeCrit.SourceRef, activeCrit.ApprovedBy, activeCrit.ApprovedAt)
                    : new CriteriaInput(1, "{}", curriculum.SourceRef);

                missionViews.Add(new MissionInput(m.MissionKey, m.Ordinal, m.Title, m.Instructions, m.Method, critView));
            }

            var rawActivities = await dbAdapter.ListActivitiesByLessonIdAsync(l.SchoolBotLessonId, ct);
            var activityViews = rawActivities
                .Select(a => new ActivityInput(a.ActivityKey, a.Ordinal, a.Title, a.Type, a.Mode, a.Indicators, a.Config))
                .ToList();

            lessonViews.Add(new LessonInput(l.LessonKey, l.Ordinal, l.Title, l.Objective, l.Instructions, missionViews, activityViews));
        }

        CurriculumApprovalView? approval = null;
        if (string.Equals(curriculum.Lifecycle, "published", StringComparison.OrdinalIgnoreCase) && curriculum.PublishedAt.HasValue)
        {
            var audits = await dbAdapter.ListAuditEventsByEntityAsync("SchoolBotCurriculum", curriculum.SchoolBotCurriculumId, ct);
            var pubAudit = audits.FirstOrDefault(a =>
                string.Equals(a.Event, "Curriculum.Published", StringComparison.OrdinalIgnoreCase) &&
                a.CreateDate == curriculum.PublishedAt.Value);

            if (pubAudit is not null)
            {
                approval = new CurriculumApprovalView(pubAudit.Actor, pubAudit.CreateDate!.Value, pubAudit.Reason);
            }
        }

        return new CurriculumDraftView(
            Id: curriculum.SchoolBotCurriculumId,
            CurriculumKey: curriculum.CurriculumKey,
            Revision: curriculum.Revision,
            LevelId: curriculum.LevelId,
            Title: curriculum.Title,
            Lifecycle: curriculum.Lifecycle,
            SourceRef: curriculum.SourceRef,
            PublishedAt: curriculum.PublishedAt,
            UpdateDate: curriculum.UpdateDate!.Value,
            RecommendedGrades: curriculum.RecommendedGrades,
            Subjects: curriculum.Subjects,
            Lessons: lessonViews,
            Approval: approval
        );
    }

    private static CurriculumSummary ToSummary(SchoolBotCurriculumRecord c) =>
        new(
            Id: c.SchoolBotCurriculumId,
            CurriculumKey: c.CurriculumKey,
            Revision: c.Revision,
            LevelId: c.LevelId,
            Title: c.Title,
            Lifecycle: c.Lifecycle,
            SourceRef: c.SourceRef,
            PublishedAt: c.PublishedAt,
            UpdateDate: c.UpdateDate!.Value
        );

}
