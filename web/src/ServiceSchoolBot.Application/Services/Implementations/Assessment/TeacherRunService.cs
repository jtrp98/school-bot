using ServiceSchoolBot.Application.Services.Implementations.WeightedScore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Services.Implementations.Assessment;

public sealed class TeacherRunService(IAssessmentRepository repository, ICurriculumRepository curricula,
    IAssessmentAuthority authority, TimeProvider clock)
{
    public Task<SchoolBotCourseRunRecord> GetRunAsync(AssessmentPrincipalContext principal, int runId, CancellationToken ct = default) =>
        AuthorizeAsync(repository, principal, runId, null, ct);
    public Task<SchoolBotRetakeGrantRecord> GrantRetakeAsync(AssessmentPrincipalContext principal, int runId,
        int activityId, int learnerId, string reason, Guid key, CancellationToken ct = default)
    {
        if (learnerId <= 0 || string.IsNullOrWhiteSpace(reason) || reason.Length > 1000) QuestionBankValidation.Invalid();
        return CommitAsync(principal, "grant_retake", key, JsonSerializer.Serialize(new { runId, activityId, learnerId, reason }),
            async (scope, token) =>
            {
                var run = await AuthorizeAsync(scope, principal, runId, learnerId, token);
                if (run.Lifecycle != "active") throw new DomainRuleException(ErrorCodes.StateConflict, "Course run is not active.");
                await new AssessmentReadService(scope, authority, clock, scope).GetActivityPolicyAsync(principal, runId, activityId, token);
                var now = await scope.GetAssessmentTimeAsync(token);
                return await scope.WriteRetakeGrantAsync(new(0, run.SchoolId, runId, activityId, learnerId,
                    reason, null, principal.EmployeeId!.Value, now), Audit(principal, "RetakeGrant.Created", "SchoolBotRetakeGrant", now, reason), token);
            }, g => g.Id, ct);
    }

    public Task<SchoolBotCourseRunActivityRecord> UpdateActivityPolicyAsync(AssessmentPrincipalContext principal,
        int runId, int activityId, int? timeLimitSeconds, DateTimeOffset? opensAt, DateTimeOffset? closesAt,
        string? etag, Guid key, CancellationToken ct = default)
    {
        if (timeLimitSeconds is <= 0 || opensAt is not null && closesAt is not null && opensAt >= closesAt)
            QuestionBankValidation.Invalid();
        return CommitAsync(principal, "update_activity_policy", key,
            JsonSerializer.Serialize(new { runId, activityId, timeLimitSeconds, opensAt, closesAt, etag }),
            async (scope, token) =>
            {
                var run = await AuthorizeAsync(scope, principal, runId, null, token);
                if (run.Lifecycle is "closing" or "closed") throw new DomainRuleException(ErrorCodes.StateConflict, "Course run is closed.");
                var setting = await scope.GetCourseRunActivityAsync(runId, activityId, token)
                    ?? throw new DomainRuleException(ErrorCodes.NotFound, "Activity policy not found.");
                Match(etag, setting.UpdateDate);
                var effective = await new AssessmentReadService(scope, authority, clock, scope)
                    .GetActivityPolicyAsync(principal, runId, activityId, token);
                var now = await scope.GetAssessmentTimeAsync(token);
                var updated = setting with { OpensAt = opensAt, ClosesAt = closesAt,
                    PolicyOverride = JsonSerializer.Serialize(effective.EffectivePolicy with { TimeLimitSeconds = timeLimitSeconds }),
                    UpdateBy = principal.EmployeeId, UpdateDate = Next(now, setting.UpdateDate) };
                return await scope.WriteActivityPolicyAsync(updated,
                    Audit(principal, "CourseRunActivity.Updated", "SchoolBotCourseRunActivity", now, "Teacher time/window override"), token);
            }, s => s.SchoolBotCourseRunActivityId, ct);
    }

    public Task<SchoolBotScoringPolicyRecord> SetScoringPolicyAsync(AssessmentPrincipalContext principal, int runId,
        ScoringPolicy policy, string? etag, Guid key, CancellationToken ct = default) =>
        CommitAsync(principal, "set_scoring_policy", key, JsonSerializer.Serialize(new { runId, policy, etag }), async (scope, token) =>
        {
            var run = await AuthorizeAsync(scope, principal, runId, null, token);
            Match(etag, run.UpdateDate);
            if (run.Lifecycle != "planned") throw new DomainRuleException(ErrorCodes.StateConflict, "Scoring policy is frozen after activation.");
            var activities = await ActivitiesAsync(scope, run.CurriculumId, token);
            ValidateWeights(policy, activities);
            var previous = run.CurrentPolicyId is { } policyId ? await scope.GetScoringPolicyAsync(policyId, token) : null;
            var now = await scope.GetAssessmentTimeAsync(token);
            var saved = await scope.WriteScoringPolicyAsync(new(0, run.SchoolId, runId,
                (previous?.Revision ?? 0) + 1, policy, principal.EmployeeId!.Value, now),
                Audit(principal, "ScoringPolicy.Created", "SchoolBotScoringPolicy", now, "Course scoring policy"), token);
            await scope.WriteCourseRunAsync(run with { CurrentPolicyId = saved.Id, UpdateDate = Next(now, run.UpdateDate), UpdateBy = principal.EmployeeId },
                Audit(principal, "CourseRun.PolicyChanged", "SchoolBotCourseRun", now, "Scoring policy selected"), token);
            return saved;
        }, s => s.Id, ct);

    public Task<SchoolBotCourseRunRecord> ActivateAsync(AssessmentPrincipalContext principal, int runId,
        string? etag, Guid key, CancellationToken ct = default) => CommitAsync(principal, "activate_course_run", key,
        JsonSerializer.Serialize(new { runId, etag }), async (scope, token) =>
        {
            var run = await AuthorizeAsync(scope, principal, runId, null, token);
            Match(etag, run.UpdateDate);
            if (run.Lifecycle != "planned" || run.CurrentPolicyId is null)
                throw new DomainRuleException(ErrorCodes.PolicyNotConfigured, "A planned course with a scoring policy is required.");
            var policy = await scope.GetScoringPolicyAsync(run.CurrentPolicyId.Value, token)
                ?? throw new DomainRuleException(ErrorCodes.PolicyNotConfigured, "Scoring policy is unavailable.");
            if (policy.CourseRunId != runId || policy.SchoolId != run.SchoolId)
                throw new DomainRuleException(ErrorCodes.SchoolMismatch, "Scoring policy scope mismatch.");
            ValidateWeights(policy.Policy, await ActivitiesAsync(scope, run.CurriculumId, token));
            var now = await scope.GetAssessmentTimeAsync(token);
            return await scope.WriteCourseRunAsync(run with { Lifecycle = "active", UpdateDate = Next(now, run.UpdateDate), UpdateBy = principal.EmployeeId },
                Audit(principal, "CourseRun.Activated", "SchoolBotCourseRun", now, "Policy frozen at activation"), token);
        }, r => r.SchoolBotCourseRunId, ct);

    private async Task<SchoolBotCourseRunRecord> AuthorizeAsync(IAssessmentRepository scope,
        AssessmentPrincipalContext principal, int runId, int? learnerId, CancellationToken ct)
    {
        if (principal.Session.Channel != "school_web" || principal.ProfileKind != "employee" || principal.EmployeeId is null)
            throw new DomainRuleException(ErrorCodes.ChannelNotAllowed, "Teacher web channel is required.");
        if (!principal.Session.IsAuthConfigured || principal.Session.IsSessionExpired || string.IsNullOrWhiteSpace(principal.Session.PrincipalId))
            throw new DomainRuleException(ErrorCodes.Unauthorized, "Teacher session is required.");
        if (!principal.Session.HasActiveSchoolLicense) throw new DomainRuleException(ErrorCodes.LicenseRequired, "Active license is required.");
        var run = await scope.GetCourseRunByIdAsync(runId, ct)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Course run not found.");
        if (run.SchoolId != principal.Session.SchoolId) throw new DomainRuleException(ErrorCodes.NotFound, "Course run not found.");
        var allowed = learnerId is { } learner ? await authority.CanReviewLearnerAsync(principal, run, learner, ct)
            : await authority.CanReadRunAsync(principal, run, ct);
        if (!allowed) throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Teacher assignment is required.");
        return run;
    }

    private async Task<T> CommitAsync<T>(AssessmentPrincipalContext principal, string operation, Guid key, string payload,
        Func<ISchoolBotRepositoryScope, CancellationToken, Task<T>> mutate, Func<T, int> resource, CancellationToken ct)
    {
        if (key == Guid.Empty) QuestionBankValidation.Invalid();
        if (repository is not IIdempotentSchoolBotCommit commit) throw new DomainRuleException(ErrorCodes.ServiceUnavailable, "Atomic storage is required.");
        var command = new IdempotencyCommand(checked((int?)principal.Session.SchoolId), principal.Session.PrincipalId!, operation,
            key, Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(payload))));
        var result = await commit.CommitWithIdempotencyAsync(command, async (scope, _, _, token) =>
        {
            var current = principal.Session.RecheckAsync is null ? principal.Session : await principal.Session.RecheckAsync(scope, token);
            if (!current.HasActiveSchoolLicense || current.IsSessionExpired || current.PrincipalId != principal.Session.PrincipalId ||
                current.SchoolId != principal.Session.SchoolId || current.Channel != "school_web")
                throw new DomainRuleException(ErrorCodes.Unauthorized, "Teacher authorization changed.");
            using var request = JsonDocument.Parse(payload);
            var root = request.RootElement;
            await AuthorizeAsync(scope, principal with { Session = current }, root.GetProperty("runId").GetInt32(),
                root.TryGetProperty("learnerId", out var learner) ? learner.GetInt32() : null, token);
        }, async (scope, _, token) =>
        {
            var body = await mutate(scope, token);
            return new MutationResult<T>(body, 200, resource(body), null);
        }, ct);
        return result.Result.Body;
    }

    private static void Match(string? etag, DateTimeOffset? updated)
    {
        if (string.IsNullOrWhiteSpace(etag)) throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match is required.");
        if (!OpaqueConcurrencyToken.Matches(etag, updated)) throw new DomainRuleException(ErrorCodes.VersionConflict, "Course configuration changed.");
    }

    private static DateTimeOffset Next(DateTimeOffset now, DateTimeOffset? previous) => now > previous ? now : previous!.Value.AddTicks(10);
    private static SchoolBotAuditEventRecord Audit(AssessmentPrincipalContext principal, string action, string entity,
        DateTimeOffset now, string reason) => new(0, principal.Session.PrincipalId!, action, entity, 0, Guid.NewGuid(),
            reason, false, principal.EmployeeId, now, principal.EmployeeId, now, checked((int?)principal.Session.SchoolId));

    internal static async Task<IReadOnlyList<SchoolBotActivityRecord>> ActivitiesAsync(ICurriculumRepository scope,
        int curriculumId, CancellationToken ct)
    {
        var activities = new List<SchoolBotActivityRecord>();
        foreach (var lesson in await scope.ListLessonsByCurriculumIdAsync(curriculumId, ct))
            activities.AddRange((await scope.ListActivitiesByLessonIdAsync(lesson.SchoolBotLessonId, ct))
                .Where(a => !a.IsDelete && a.Type != "instruction"));
        return activities;
    }

    private static void ValidateWeights(ScoringPolicy policy, IReadOnlyList<SchoolBotActivityRecord> activities)
    {
        if (policy is null || policy.FormulaVersion != "weighted-normalized-v1" || policy.ActivityWeights is null ||
            policy.ActivityWeights.Count != activities.Count ||
            policy.ActivityWeights.Select(w => w.ActivityId).Distinct().Count() != activities.Count ||
            policy.ActivityWeights.Any(w => !activities.Any(a => a.SchoolBotActivityId == w.ActivityId)))
            throw new DomainRuleException(ErrorCodes.PolicyNotConfigured, "Policy must cover every scoring activity exactly once.");
        WeightedScoreCalculator.Calculate(policy.ActivityWeights.Select(w => new WeightedActivityScore(w.ActivityId,
            0, 1, w.WeightBps, activities.Single(a => a.SchoolBotActivityId == w.ActivityId).Type == "quiz")).ToArray(), policy.QuizCounts);
    }

    public async Task<LearningScoreTotal> GetTotalAsync(AssessmentPrincipalContext principal, int runId,
        int learnerId, CancellationToken ct = default)
    {
        var run = await AuthorizeAsync(repository, principal, runId, learnerId, ct);
        var policy = run.CurrentPolicyId is { } policyId ? await repository.GetScoringPolicyAsync(policyId, ct) : null;
        if (policy is null || policy.SchoolId != run.SchoolId || policy.CourseRunId != runId)
            throw new DomainRuleException(ErrorCodes.PolicyNotConfigured, "Frozen scoring policy is unavailable.");
        var activities = await ActivitiesAsync(curricula, run.CurriculumId, ct);
        ValidateWeights(policy.Policy, activities);
        var sources = new List<LearningScoreSource>();
        var gaps = new List<LearningScoreGap>();
        var weighted = new List<WeightedActivityScore>();
        var complete = true;
        foreach (var weight in policy.Policy.ActivityWeights)
        {
            var activity = activities.Single(a => a.SchoolBotActivityId == weight.ActivityId);
            LearningScoreSource? source = null;
            var missingReason = "SOURCE_RESULT_MISSING";
            if (activity.Type is "quiz" or "test")
            {
                var assessmentTime = await repository.GetAssessmentTimeAsync(ct);
                foreach (var pending in (await repository.ListAttemptsAsync(runId, activity.SchoolBotActivityId, learnerId, ct))
                    .Where(a => a.State == "started" && a.DeadlineAt <= assessmentTime || a.State == "expired" && a.EarnedScore is null))
                {
                    if (repository is ISchoolBotRepositoryScope scope && repository is IIdempotentSchoolBotCommit { IsInAtomicCommit: true })
                        await AttemptFinalizer.FinalizeInTransactionAsync(scope, pending, ct);
                    else
                        await CommitAsync(principal, "finalize_scoring_attempt",
                            new Guid(SHA256.HashData(Encoding.UTF8.GetBytes($"score-expiry:{pending.SchoolBotAttemptId}")).AsSpan(0, 16)),
                            JsonSerializer.Serialize(new { runId, learnerId, attemptId = pending.SchoolBotAttemptId }), async (transaction, token) =>
                            {
                                var current = await transaction.GetAttemptByIdAsync(pending.SchoolBotAttemptId, token)
                                    ?? throw new DomainRuleException(ErrorCodes.NotFound, "Attempt not found.");
                                await AttemptFinalizer.FinalizeInTransactionAsync(transaction, current, token);
                                return current;
                            }, a => a.SchoolBotAttemptId, ct);
                }
                var allAttempts = (await repository.ListAttemptsAsync(runId, activity.SchoolBotActivityId, learnerId, ct))
                    .Where(a => a.SchoolId == run.SchoolId).ToArray();
                if (allAttempts.Length > 0) missingReason = "SOURCE_RESULT_UNGRADED";
                var attempts = allAttempts.Where(a => a.State is "submitted" or "expired" && a.EarnedScore is not null).ToArray();
                if (attempts.Length > 0)
                {
                    var selection = activity.Type == "test" ? "best" : JsonSerializer.Deserialize<AttemptSnapshot>(attempts[0].Snapshot)!.EffectivePolicy.Selection;
                    var selected = selection switch
                    {
                        "best" => attempts.OrderByDescending(a => a.EarnedScore).ThenBy(a => a.Sequence).First(),
                        "first" => attempts.MinBy(a => a.Sequence)!,
                        _ => attempts.MaxBy(a => a.Sequence)!
                    };
                    source = new("attempt", selected.SchoolBotAttemptId, selected.UpdateDate!.Value,
                        weight.ActivityId, selected.EarnedScore!.Value, selected.MaxScore, weight.WeightBps);
                }
            }
            else
            {
                var missionId = ProjectActivityReference.ResolveMissionId(activity.Config,
                    await curricula.ListMissionsByLessonIdAsync(activity.LessonId, ct));
                foreach (var submission in await repository.ListSubmissionsAsync(run.SchoolId, learnerId, missionId, ct))
                {
                    var audits = await repository.ListAuditEventsByEntityAsync("SchoolBotSubmission", submission.Id, ct);
                    var accepted = audits.SingleOrDefault(a => a.Event == "Submission.Accepted");
                    if (accepted is null) throw new DomainRuleException("SOURCE_SCOPE_UNPROVEN", "Submission run evidence is missing.");
                    using var reason = JsonDocument.Parse(accepted.Reason);
                    if (reason.RootElement.GetProperty("course_run_id").GetInt32() != runId) continue;
                    var project = await repository.GetLearningProjectAsync(submission.ProjectRevisionId, ct);
                    if (project is null || project.Value.Project.OwnerKind != "student" ||
                        project.Value.Project.OwnerId != learnerId || project.Value.Project.SchoolId != run.SchoolId ||
                        project.Value.Revision.SchoolId != run.SchoolId)
                        throw new DomainRuleException("SOURCE_SCOPE_UNPROVEN", "Submission must belong to this student and school.");
                    var assessment = (await repository.ListSubmissionAssessmentsAsync(submission.Id, ct))
                        .SingleOrDefault(a => a.Id == submission.CurrentAssessmentId);
                    missingReason = submission.State == "failed" ? "SOURCE_RESULT_ERROR" : "SOURCE_RESULT_UNGRADED";
                    if (assessment?.EarnedScore is { } earned && assessment.MaxScore is { } max)
                        source = new("assessment", assessment.Id, assessment.CreatedAt, weight.ActivityId, earned, max, weight.WeightBps);
                    break;
                }
            }
            if (source is not null) sources.Add(source);
            else
            {
                gaps.Add(new(weight.ActivityId, missingReason));
                if (weight.WeightBps > 0) complete = false;
            }
            weighted.Add(new(weight.ActivityId, source?.Earned ?? 0, source?.MaxScore ?? 1,
                weight.WeightBps, activity.Type == "quiz"));
        }
        return new(runId, learnerId, policy.Id, WeightedScoreCalculator.Calculate(weighted, policy.Policy.QuizCounts), complete, sources, gaps);
    }

}

