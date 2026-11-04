using ServiceSchoolBot.Application.Common.Model.Assessment;
using System.Text.Json;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;

namespace ServiceSchoolBot.Application.Services.Implementations.Assessment;

public sealed partial class AssessmentReadService
{
    public async Task<ProgressView> GetProgressAsync(
        AssessmentPrincipalContext principal, int? courseRunId = null, int? curriculumId = null,
        string? mode = null, CancellationToken ct = default)
    {
        ValidateStudent(principal);
        if (principal.LearnerId is <= 0 || principal.Session.SchoolId is null or <= 0 ||
            courseRunId is <= 0 || curriculumId is <= 0 ||
            (courseRunId is null && curriculumId is null) || mode is not (null or "block" or "arm" or "code"))
            throw new DomainRuleException(ErrorCodes.InvalidQuery, "Invalid progress filters.");
        if (!principal.Session.HasActiveSchoolLicense)
            throw new DomainRuleException(ErrorCodes.LicenseRequired, "An active school license is required.");

        var run = courseRunId is { } runId ? await ReadRunAsync(principal, runId, true, ct) : null;
        if (run is not null && curriculumId is { } filter && filter != run.CurriculumId)
            throw new DomainRuleException(ErrorCodes.InvalidQuery, "Curriculum does not match the course run.");
        var curriculum = await curricula.GetCurriculumByIdAsync(run?.CurriculumId ?? curriculumId!.Value, ct)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Curriculum not found.");
        if (curriculum.SchoolId is not null || curriculum.Lifecycle != "published")
            throw new DomainRuleException(ErrorCodes.StateConflict, "A published central curriculum is required.");

        var activities = new List<ActivityProgressView>();
        var missions = new List<MissionProgressView>();
        var lessons = await curricula.ListLessonsByCurriculumIdAsync(curriculum.SchoolBotCurriculumId, ct);
        foreach (var lesson in lessons)
        {
            var includedMissions = new HashSet<int>();
            foreach (var activity in await curricula.ListActivitiesByLessonIdAsync(lesson.SchoolBotLessonId, ct))
            {
                if (mode is not null && activity.Mode != mode) continue;
                SelectedAttemptView? selected = null;
                if (run is not null && activity.Type is "quiz" or "test")
                    selected = await SelectAttemptAsync(
                        principal, run, activity.SchoolBotActivityId, activity.Type, ct);
                activities.Add(new(activity.SchoolBotActivityId, activity.Type, activity.Mode, null, selected));
                if (activity.Type == "project")
                {
                    try
                    {
                        using var config = JsonDocument.Parse(activity.Config);
                        includedMissions.Add(config.RootElement.GetProperty("mission_id").GetInt32());
                    }
                    catch (Exception ex) when (ex is JsonException or KeyNotFoundException or
                        InvalidOperationException or FormatException)
                    {
                        throw new DomainRuleException(ErrorCodes.StateConflict, "Stored activity is unavailable.");
                    }
                }
            }
            foreach (var mission in await curricula.ListMissionsByLessonIdAsync(lesson.SchoolBotLessonId, ct))
            {
                if (mode is null || includedMissions.Contains(mission.SchoolBotMissionId))
                    missions.Add(new(mission.SchoolBotMissionId, null));
            }
        }
        return new(principal.LearnerId!.Value, courseRunId, curriculum.SchoolBotCurriculumId,
            curriculum.LevelId, activities, missions, clock.GetUtcNow());
    }

    private async Task<SelectedAttemptView?> SelectAttemptAsync(
        AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run, int activityId,
        string activityType, CancellationToken ct)
    {
        var attempts = (await dbAdapter.ListAttemptsAsync(run.SchoolBotCourseRunId, activityId,
            principal.LearnerId!.Value, ct)).Where(row => row.SchoolId == run.SchoolId &&
                row.State is "submitted" or "expired" && row.EarnedScore is not null).ToArray();
        if (attempts.Length == 0) return null;
        var policy = await GetActivityPolicyAsync(principal, run.SchoolBotCourseRunId, activityId, ct);
        var selection = activityType == "test" ? "best" : policy.EffectivePolicy.Selection;
        var selected = selection switch
        {
            "best" => attempts.OrderByDescending(row => row.EarnedScore).ThenBy(row => row.Sequence).First(),
            "first" => attempts.MinBy(row => row.Sequence)!,
            _ => attempts.MaxBy(row => row.Sequence)!
        };
        var snapshot = Snapshot(selected);
        return new(selected.SchoolBotAttemptId, selected.Sequence, selected.State,
            CanReveal(selected, snapshot) ? Score(selected.EarnedScore) : null, Score(selected.MaxScore)!);
    }
}
