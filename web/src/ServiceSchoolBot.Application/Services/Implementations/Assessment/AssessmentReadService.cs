using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using System.Globalization;
using System.Text.Json;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;

namespace ServiceSchoolBot.Application.Services.Implementations.Assessment;

public sealed partial class AssessmentReadService(
    IAssessmentRepository dbAdapter, IAssessmentAuthority authority, TimeProvider clock,
    ICurriculumRepository curricula)
{
    public async Task<ActivityPolicyView> GetActivityPolicyAsync(
        AssessmentPrincipalContext principal, int courseRunId, int activityId, CancellationToken ct = default)
    {
        var run = await ReadRunAsync(principal, courseRunId, false, ct);
        if (activityId <= 0) QuestionBankValidation.Invalid();
        var setting = await dbAdapter.GetCourseRunActivityAsync(courseRunId, activityId, ct)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Activity policy not found.");
        if (setting.SchoolId != run.SchoolId)
            throw new DomainRuleException(ErrorCodes.SchoolMismatch, "School context mismatch.");
        var lessons = await curricula.ListLessonsByCurriculumIdAsync(run.CurriculumId, ct);
        foreach (var lesson in lessons)
        {
            var activity = (await curricula.ListActivitiesByLessonIdAsync(lesson.SchoolBotLessonId, ct))
                .SingleOrDefault(row => row.SchoolBotActivityId == activityId && !row.IsDelete);
            if (activity is null) continue;
            if (activity.Type is not ("quiz" or "test"))
                throw new DomainRuleException(ErrorCodes.StateConflict, "Activity is not an assessment.");
            try
            {
                using var config = JsonDocument.Parse(activity.Config);
                var policy = setting.PolicyOverride is null
                    ? JsonSerializer.Deserialize<AssessmentPolicy>(config.RootElement.GetProperty("default_policy"))
                    : JsonSerializer.Deserialize<AssessmentPolicy>(setting.PolicyOverride);
                if (policy is null || policy.MaxAttempts is <= 0 || policy.TimeLimitSeconds is <= 0 ||
                    policy.Selection is not ("best" or "first" or "latest") ||
                    policy.Reveal is not ("after_submit" or "after_close"))
                    throw new DomainRuleException(ErrorCodes.PolicyNotConfigured, "Assessment policy is not configured.");
                return new(courseRunId, activityId, setting.OpensAt, setting.ClosesAt, policy,
                    setting.UpdateDate ?? activity.UpdateDate ?? run.UpdateDate!.Value);
            }
            catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
            {
                throw new DomainRuleException(ErrorCodes.PolicyNotConfigured, "Assessment policy is not configured.");
            }
        }
        throw new DomainRuleException(ErrorCodes.NotFound, "Activity not found in course run.");
    }

    public async Task<AttemptView> GetAttemptAsync(
        AssessmentPrincipalContext principal, int id, CancellationToken ct = default)
    {
        ValidateStudent(principal);
        if (id <= 0) QuestionBankValidation.Invalid();
        var attempt = await dbAdapter.GetAttemptByIdAsync(id, ct)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Attempt not found.");
        var run = await ReadRunAsync(principal, attempt.CourseRunId, true, ct);
        if (attempt.SchoolId != run.SchoolId || attempt.LearnerId != principal.LearnerId)
            throw new DomainRuleException(ErrorCodes.NotFound, "Attempt not found.");
        return Project(attempt);
    }

    public async Task<AttemptListResponse> ListAttemptsAsync(
        AssessmentPrincipalContext principal, int courseRunId, int? activityId = null, string? state = null,
        int limit = 50, string? cursor = null, CancellationToken ct = default)
    {
        var run = await ReadRunAsync(principal, courseRunId, true, ct);
        var scope = JsonSerializer.Serialize(new
        {
            principal.Session.PrincipalId, principal.Session.SchoolId, principal.LearnerId, courseRunId, activityId, state
        });
        var offset = AssessmentPagination.Offset(limit, cursor, scope);
        if (state is not (null or "started" or "submitted" or "expired" or "voided"))
            throw new DomainRuleException(ErrorCodes.InvalidQuery, "Invalid attempt state.");
        var attempts = (await dbAdapter.ListAttemptsAsync(courseRunId, activityId, principal.LearnerId!.Value, ct))
            .Where(row => row.SchoolId == run.SchoolId && (state is null || row.State == state)).ToArray();
        var items = attempts.Skip(offset).Take(limit).Select(row =>
        {
            var snapshot = Snapshot(row);
            return new AttemptSummary(row.SchoolBotAttemptId, row.UpdateDate!.Value, row.Sequence, row.State,
                row.CourseRunId, row.ActivityId, row.StartedAt, row.DeadlineAt, row.SubmittedAt,
                CanReveal(row, snapshot) ? Score(row.EarnedScore) : null, Score(row.MaxScore)!);
        }).ToArray();
        return new(items, AssessmentPagination.Next(offset, items.Length, attempts.Length, scope));
    }

    private AttemptView Project(SchoolBotAttemptRecord attempt)
    {
        try
        {
            var snapshot = Snapshot(attempt);
            var answers = JsonSerializer.Deserialize<AssessmentAnswer[]>(attempt.Answers)
                ?? throw new JsonException();
            var questions = snapshot.QuestionOrder.Select(key =>
            {
                var question = snapshot.Questions.Single(row => row.QuestionKey == key);
                var order = snapshot.OptionOrder.Single(row => row.QuestionKey == key);
                return new PublicQuestion(key, question.Kind, question.Prompt,
                    order.OptionKeys.Select(optionKey => question.Options.Single(option => option.OptionKey == optionKey))
                        .ToArray(), question.MaxScore);
            }).ToArray();
            var reveal = CanReveal(attempt, snapshot);
            var feedback = reveal ? snapshot.QuestionOrder.Select(key =>
            {
                var question = snapshot.Questions.Single(row => row.QuestionKey == key);
                var answer = answers.SingleOrDefault(row => row.QuestionKey == key);
                var correct = answer is not null && answer.SelectedOptionKeys.Count == 1 &&
                    question.CorrectOptionKeys.SequenceEqual(answer.SelectedOptionKeys);
                return new AttemptFeedback(key, correct, question.CorrectOptionKeys);
            }).ToArray() : null;
            return new(attempt.SchoolBotAttemptId, attempt.UpdateDate!.Value, attempt.Sequence, attempt.State,
                attempt.ActivityId, attempt.CourseRunId, attempt.StartedAt, attempt.DeadlineAt, attempt.SubmittedAt,
                questions, reveal ? Score(attempt.EarnedScore) : null, Score(attempt.MaxScore)!, feedback, answers,
                snapshot.EffectivePolicy, snapshot.OpensAt, snapshot.ClosesAt, snapshot.BankId, snapshot.BankRevision,
                snapshot.CurriculumRevision, snapshot.PolicyId, snapshot.EvaluatorVersion, clock.GetUtcNow());
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or NullReferenceException)
        {
            throw new DomainRuleException(ErrorCodes.StateConflict, "Stored attempt snapshot is unavailable.");
        }
    }

    private static AttemptSnapshot Snapshot(SchoolBotAttemptRecord attempt)
    {
        try
        {
            return JsonSerializer.Deserialize<AttemptSnapshot>(attempt.Snapshot) ?? throw new JsonException();
        }
        catch (JsonException)
        {
            throw new DomainRuleException(ErrorCodes.StateConflict, "Stored attempt snapshot is unavailable.");
        }
    }

    private bool CanReveal(SchoolBotAttemptRecord attempt, AttemptSnapshot snapshot) =>
        attempt.State is "submitted" or "expired" && attempt.EarnedScore is not null &&
        (snapshot.EffectivePolicy.Reveal == "after_submit" ||
            (snapshot.EffectivePolicy.Reveal == "after_close" && snapshot.ClosesAt is { } closes &&
                clock.GetUtcNow() >= closes));

    private static string? Score(decimal? score) => score?.ToString("0.######", CultureInfo.InvariantCulture);

    private async Task<SchoolBotCourseRunRecord> ReadRunAsync(
        AssessmentPrincipalContext principal, int id, bool studentOnly, CancellationToken ct)
    {
        ValidateSession(principal);
        if (studentOnly || principal.Session.Channel == "desktop") ValidateStudent(principal);
        else if (principal.Session.Channel != "school_web")
            throw new DomainRuleException(ErrorCodes.ChannelNotAllowed, "Channel is not allowed.");
        if (id <= 0) QuestionBankValidation.Invalid();
        var run = await dbAdapter.GetCourseRunByIdAsync(id, ct)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Course run not found.");
        if (principal.Session.SchoolId != run.SchoolId)
            throw new DomainRuleException(ErrorCodes.SchoolMismatch, "School context mismatch.");
        if (!principal.Session.HasActiveSchoolLicense)
            throw new DomainRuleException(ErrorCodes.LicenseRequired, "An active school license is required.");
        if (!await authority.CanReadRunAsync(principal, run, ct))
            throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Course run access is not allowed.");
        return run;
    }

    private static void ValidateSession(AssessmentPrincipalContext principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        var access = CurriculumAuthorizationService.EvaluateReadCurriculum(principal.Session, true, DateTimeOffset.MinValue);
        if (!access.IsAllowed) throw new DomainRuleException(access.ErrorCode!, access.Message!);
    }

    private static void ValidateStudent(AssessmentPrincipalContext principal)
    {
        ValidateSession(principal);
        if (principal.Session.Channel != "desktop")
            throw new DomainRuleException(ErrorCodes.ChannelNotAllowed, "Desktop channel is required.");
        if (principal.ProfileKind != "student" || principal.LearnerId is null)
            throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Student profile is required.");
    }
}
