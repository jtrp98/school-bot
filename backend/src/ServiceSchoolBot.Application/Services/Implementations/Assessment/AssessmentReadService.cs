using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Concurrency;
using System.Text;
using System.Security.Cryptography;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using System.Globalization;
using System.Text.Json;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;

namespace ServiceSchoolBot.Application.Services.Implementations.Assessment;

public sealed class AssessmentReadService(
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
        var recovered = await RecoverAttemptAsync(principal, attempt, ct);
        return Project(recovered, await dbAdapter.GetAssessmentTimeAsync(ct));
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
            .Where(row => row.SchoolId == run.SchoolId).ToArray();
        for (var i = 0; i < attempts.Length; i++)
            attempts[i] = await RecoverAttemptAsync(principal, attempts[i], ct);
        attempts = attempts.Where(row => state is null || row.State == state)
            .OrderByDescending(row => row.StartedAt).ThenByDescending(row => row.SchoolBotAttemptId).ToArray();
        var now = await dbAdapter.GetAssessmentTimeAsync(ct);
        var items = attempts.Skip(offset).Take(limit).Select(row =>
        {
            var snapshot = Snapshot(row);
            return new AttemptSummary(row.SchoolBotAttemptId, row.UpdateDate!.Value, row.Sequence, row.State,
                row.CourseRunId, row.ActivityId, row.StartedAt, row.DeadlineAt, row.SubmittedAt,
                CanReveal(row, snapshot, now) ? Score(row.EarnedScore) : null, Score(row.MaxScore)!);
        }).ToArray();
        return new(items, AssessmentPagination.Next(offset, items.Length, attempts.Length, scope));
    }

    private AttemptView Project(SchoolBotAttemptRecord attempt, DateTimeOffset now)
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
            var reveal = CanReveal(attempt, snapshot, now);
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
                snapshot.CurriculumRevision, snapshot.PolicyId, snapshot.EvaluatorVersion, now);
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

    private static bool CanReveal(SchoolBotAttemptRecord attempt, AttemptSnapshot snapshot, DateTimeOffset now) =>
        attempt.State is "submitted" or "expired" && attempt.EarnedScore is not null &&
        (snapshot.EffectivePolicy.Reveal == "after_submit" ||
            (snapshot.EffectivePolicy.Reveal == "after_close" && snapshot.ClosesAt is { } closes &&
                now >= closes));

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
        var submissions = await dbAdapter.ListSubmissionsAsync(checked((int)principal.Session.SchoolId!.Value),
            principal.LearnerId, null, ct);
        async Task<JsonElement?> Latest(int missionId)
        {
            foreach (var candidate in submissions.Where(s => s.MissionId == missionId))
            {
                if (run is not null)
                {
                    var audits = await dbAdapter.ListAuditEventsByEntityAsync("SchoolBotSubmission", candidate.Id, ct);
                    var accepted = audits.SingleOrDefault(a => a.Event == "Submission.Accepted");
                    if (accepted is null) continue;
                    using var reason = JsonDocument.Parse(accepted.Reason!);
                    if (reason.RootElement.GetProperty("course_run_id").GetInt32() != run.SchoolBotCourseRunId) continue;
                }
                var assessment = (await dbAdapter.ListSubmissionAssessmentsAsync(candidate.Id, ct))
                    .SingleOrDefault(a => a.Id == candidate.CurrentAssessmentId);
                return JsonSerializer.SerializeToElement(new SubmissionView(candidate, assessment),
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            }
            return null;
        }
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
                JsonElement? latestSubmission = null;
                if (activity.Type == "project")
                {
                    try
                    {
                        var missionId = ProjectActivityReference.ResolveMissionId(activity.Config,
                            await curricula.ListMissionsByLessonIdAsync(lesson.SchoolBotLessonId, ct));
                        includedMissions.Add(missionId);
                        latestSubmission = await Latest(missionId);
                    }
                    catch (Exception ex) when (ex is JsonException or KeyNotFoundException or
                        InvalidOperationException or FormatException)
                    {
                        throw new DomainRuleException(ErrorCodes.StateConflict, "Stored activity is unavailable.");
                    }
                }
                activities.Add(new(activity.SchoolBotActivityId, activity.Type, activity.Mode, latestSubmission, selected));
            }
            foreach (var mission in await curricula.ListMissionsByLessonIdAsync(lesson.SchoolBotLessonId, ct))
            {
                if (mode is null || includedMissions.Contains(mission.SchoolBotMissionId))
                    missions.Add(new(mission.SchoolBotMissionId, await Latest(mission.SchoolBotMissionId)));
            }
        }
        return new(principal.LearnerId!.Value, courseRunId, curriculum.SchoolBotCurriculumId,
            curriculum.LevelId, activities, missions, clock.GetUtcNow());
    }

    private async Task<SelectedAttemptView?> SelectAttemptAsync(
        AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run, int activityId,
        string activityType, CancellationToken ct)
    {
        var rows = (await dbAdapter.ListAttemptsAsync(run.SchoolBotCourseRunId, activityId,
            principal.LearnerId!.Value, ct)).Where(row => row.SchoolId == run.SchoolId).ToArray();
        for (var i = 0; i < rows.Length; i++) rows[i] = await RecoverAttemptAsync(principal, rows[i], ct);
        var attempts = rows.Where(row => row.State is "submitted" or "expired" && row.EarnedScore is not null).ToArray();
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
            CanReveal(selected, snapshot, await dbAdapter.GetAssessmentTimeAsync(ct)) ? Score(selected.EarnedScore) : null, Score(selected.MaxScore)!);
    }


    public async Task<AttemptView> StartAttemptAsync(AssessmentPrincipalContext principal,
        int courseRunId, int activityId, Guid key, CancellationToken ct = default)
    {
        var saved = await CommitAttemptAsync(principal, "start_attempt", key, $"{courseRunId}|{activityId}",
            courseRunId, async (scope, now, token) =>
            {
                var reader = new AssessmentReadService(scope, authority, clock, scope);
                var run = await reader.ReadRunAsync(principal, courseRunId, true, token);
                var policy = await reader.GetActivityPolicyAsync(principal, courseRunId, activityId, token);
                now = await scope.GetAssessmentTimeAsync(token);
                if (run.Lifecycle != "active" || now < run.EffectiveAt ||
                    policy.OpensAt is { } opens && now < opens || policy.ClosesAt is { } closes && now >= closes)
                    throw new DomainRuleException(ErrorCodes.StateConflict, "Assessment is not open.");
                var previous = await scope.ListAttemptsAsync(courseRunId, activityId, principal.LearnerId!.Value, token);
                foreach (var active in previous.Where(a => a.State == "started" && a.DeadlineAt is { } d && now >= d))
                {
                    var expired = AttemptEvaluation.Apply(active, null, false, now, null);
                    await scope.WriteAttemptAsync(expired, AttemptAudit(principal, "Expired", now), token);
                }
                previous = await scope.ListAttemptsAsync(courseRunId, activityId, principal.LearnerId.Value, token);
                if (previous.Any(a => a.State == "started"))
                    throw new DomainRuleException(ErrorCodes.StateConflict, "Attempt limit reached or an attempt is active.");
                SchoolBotRetakeGrantRecord? retake = null;
                if (policy.EffectivePolicy.MaxAttempts is { } max && previous.Count(a => a.State != "voided") >= max)
                {
                    retake = (await scope.ListRetakeGrantsAsync(courseRunId, activityId, principal.LearnerId.Value, token))
                        .FirstOrDefault(g => g.SchoolId == run.SchoolId && g.ConsumedAttemptId is null);
                    if (retake is null) throw new DomainRuleException(ErrorCodes.StateConflict, "Attempt limit reached.");
                }
                if (run.CurrentPolicyId is null)
                    throw new DomainRuleException(ErrorCodes.PolicyNotConfigured, "Scoring policy is required.");
                var curriculum = await scope.GetCurriculumByIdAsync(run.CurriculumId, token)
                    ?? throw new DomainRuleException(ErrorCodes.NotFound, "Curriculum not found.");
                if (curriculum.Lifecycle != "published" || curriculum.SchoolId is not null)
                    throw new DomainRuleException(ErrorCodes.StateConflict, "A published central curriculum is required.");
                var lessons = await scope.ListLessonsByCurriculumIdAsync(run.CurriculumId, token);
                SchoolBotActivityRecord? activity = null;
                foreach (var lesson in lessons)
                    activity ??= (await scope.ListActivitiesByLessonIdAsync(lesson.SchoolBotLessonId, token))
                        .SingleOrDefault(a => a.SchoolBotActivityId == activityId && !a.IsDelete);
                using var config = JsonDocument.Parse(activity!.Config);
                var bank = await scope.GetQuestionBankByIdAsync(config.RootElement.GetProperty("bank_id").GetInt32(), token)
                    ?? throw new DomainRuleException(ErrorCodes.NotFound, "Question bank not found.");
                if (bank.Lifecycle != "published" || bank.SchoolId is not null)
                    throw new DomainRuleException(ErrorCodes.StateConflict, "A published central bank is required.");
                var keys = config.RootElement.GetProperty("question_keys").EnumerateArray().Select(v => v.GetGuid()).ToArray();
                if (keys.Length == 0 || keys.Distinct().Count() != keys.Length ||
                    keys.Any(k => !bank.Questions.Any(q => q.QuestionKey == k))) QuestionBankValidation.Invalid();
                var questions = keys.Select(k => bank.Questions.Single(q => q.QuestionKey == k)).ToArray();
                QuestionBankValidation.Validate(bank.Title, questions, bank.SourceRef);
                var order = keys.ToArray();
                if (policy.EffectivePolicy.ShuffleQuestions) RandomNumberGenerator.Shuffle(order.AsSpan());
                var options = questions.Select(q =>
                {
                    var optionKeys = q.Options.Select(o => o.OptionKey).ToArray();
                    if (policy.EffectivePolicy.ShuffleOptions) RandomNumberGenerator.Shuffle(optionKeys.AsSpan());
                    return new AssessmentOptionOrder(q.QuestionKey, optionKeys);
                }).ToArray();
                now = await scope.GetAssessmentTimeAsync(token);
                if (policy.ClosesAt is { } closing && now >= closing)
                    throw new DomainRuleException(ErrorCodes.StateConflict, "Assessment is closed.");
                var snapshot = new AttemptSnapshot(bank.SchoolBotQuestionBankId, bank.Revision, curriculum.Revision,
                    run.CurrentPolicyId.Value, AttemptEvaluation.Version, questions, order, options,
                    policy.EffectivePolicy, policy.OpensAt, policy.ClosesAt);
                var attempt = new SchoolBotAttemptRecord(0, courseRunId, activityId, principal.LearnerId.Value,
                    previous.Select(a => a.Sequence).DefaultIfEmpty().Max() + 1, "started", now,
                    AttemptEvaluation.Deadline(now, policy.EffectivePolicy, policy.ClosesAt), null,
                    JsonSerializer.Serialize(snapshot), "[]", null,
                    questions.Sum(q => decimal.Parse(q.MaxScore, CultureInfo.InvariantCulture)),
                    false, principal.LearnerId, now, principal.LearnerId, now, run.SchoolId);
                var saved = await scope.WriteAttemptAsync(attempt, AttemptAudit(principal, "Started", now), token);
                if (retake is not null)
                    await scope.WriteRetakeGrantAsync(retake with { ConsumedAttemptId = saved.SchoolBotAttemptId },
                        new(0, principal.Session.PrincipalId!, "RetakeGrant.Consumed", "SchoolBotRetakeGrant", retake.Id,
                            Guid.NewGuid(), "Retake consumed by attempt start", false, null, now, null, now, run.SchoolId), token);
                return saved;
            }, ct);
        return Project(saved, await dbAdapter.GetAssessmentTimeAsync(ct));
    }

    public async Task<AttemptView> SaveAttemptAsync(AssessmentPrincipalContext principal, int id,
        IReadOnlyList<AssessmentAnswer> answers, bool submit, string? etag, Guid key, CancellationToken ct = default)
    {
        ValidateStudent(principal);
        if (id <= 0 || answers is null || answers.Count > 10000) QuestionBankValidation.Invalid();
        if (string.IsNullOrWhiteSpace(etag))
            throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match is required.");
        var existing = await OwnedAttemptAsync(principal, id, ct);
        AttemptEvaluation.ValidateAnswers(Snapshot(existing), answers);
        var saved = await CommitAttemptAsync(principal, submit ? "submit_attempt" : "save_attempt", key,
            JsonSerializer.Serialize(new { id, answers, etag }), existing.CourseRunId,
            async (scope, now, token) =>
            {
                var reader = new AssessmentReadService(scope, authority, clock, scope);
                var current = await reader.OwnedAttemptAsync(principal, id, token);
                now = await scope.GetAssessmentTimeAsync(token);
                var updated = AttemptEvaluation.Apply(current, answers, submit, now, etag);
                if (updated == current) return current;
                return await scope.WriteAttemptAsync(updated, AttemptAudit(principal,
                    updated.State == "expired" ? "Expired" : submit ? "Submitted" : "AnswersSaved", now), token);
            }, ct);
        // Commit expiry before returning the domain error; throwing inside the transaction would roll it back.
        if (saved.State == "expired")
            throw new DomainRuleException(ErrorCodes.AttemptExpired, "Attempt has expired; saved answers were graded.");
        return Project(saved, await dbAdapter.GetAssessmentTimeAsync(ct));
    }

    private async Task<SchoolBotAttemptRecord> OwnedAttemptAsync(AssessmentPrincipalContext principal,
        int id, CancellationToken ct)
    {
        ValidateStudent(principal);
        var attempt = await dbAdapter.GetAttemptByIdAsync(id, ct)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Attempt not found.");
        await ReadRunAsync(principal, attempt.CourseRunId, true, ct);
        if (attempt.SchoolId != principal.Session.SchoolId || attempt.LearnerId != principal.LearnerId)
            throw new DomainRuleException(ErrorCodes.NotFound, "Attempt not found.");
        return attempt;
    }

    private async Task<SchoolBotAttemptRecord> RecoverAttemptAsync(AssessmentPrincipalContext principal,
        SchoolBotAttemptRecord attempt, CancellationToken ct)
    {
        var authoritativeNow = await dbAdapter.GetAssessmentTimeAsync(ct);
        if (!(attempt.State == "started" && attempt.DeadlineAt is { } deadline && authoritativeNow >= deadline ||
            attempt.State == "expired" && attempt.EarnedScore is null)) return attempt;
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"attempt-expiry:{attempt.SchoolBotAttemptId}"));
        return await CommitAttemptAsync(principal, "recover_attempt", new Guid(bytes.AsSpan(0, 16)),
            attempt.SchoolBotAttemptId.ToString(CultureInfo.InvariantCulture), attempt.CourseRunId,
            async (scope, now, token) =>
            {
                var reader = new AssessmentReadService(scope, authority, clock, scope);
                var current = await reader.OwnedAttemptAsync(principal, attempt.SchoolBotAttemptId, token);
                now = await scope.GetAssessmentTimeAsync(token);
                var updated = AttemptEvaluation.Apply(current, null, false, now, null);
                return updated == current ? current : await scope.WriteAttemptAsync(updated,
                    AttemptAudit(principal, "Expired", now), token);
            }, ct);
    }

    private async Task<SchoolBotAttemptRecord> CommitAttemptAsync(AssessmentPrincipalContext principal,
        string operation, Guid key, string payload, int courseRunId,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<SchoolBotAttemptRecord>> mutate,
        CancellationToken ct)
    {
        ValidateStudent(principal);
        if (key == Guid.Empty) QuestionBankValidation.Invalid();
        if (dbAdapter is not IIdempotentSchoolBotCommit commit)
            throw new DomainRuleException(ErrorCodes.ServiceUnavailable, "Atomic assessment storage is required.");
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
        var result = await commit.CommitWithIdempotencyAsync(
            new(checked((int?)principal.Session.SchoolId), principal.Session.PrincipalId! + ":learner:" + principal.LearnerId, operation, key, hash),
            async (scope, _, _, token) =>
            {
                var reader = new AssessmentReadService(scope, authority, clock, scope);
                var current = principal.Session.RecheckAsync is null ? principal : principal with
                {
                    Session = await principal.Session.RecheckAsync(scope, token)
                };
                if (current.Session.PrincipalId != principal.Session.PrincipalId ||
                    current.Session.SchoolId != principal.Session.SchoolId)
                    throw new DomainRuleException(ErrorCodes.Unauthorized, "Principal context changed.");
                await reader.ReadRunAsync(current, courseRunId, true, token);
            }, async (scope, now, token) =>
            {
                var saved = await mutate(scope, now, token);
                return new MutationResult<SchoolBotAttemptRecord>(saved, 200, saved.SchoolBotAttemptId,
                    OpaqueConcurrencyToken.ToETag(saved.UpdateDate));
            }, ct);
        return result.Result.Body;
    }

    private static SchoolBotAuditEventRecord AttemptAudit(AssessmentPrincipalContext principal,
        string action, DateTimeOffset now) => new(0, principal.Session.PrincipalId!, "Attempt." + action,
            "SchoolBotAttempt", 0, Guid.NewGuid(), "Assessment attempt " + action, false,
            principal.LearnerId, now, principal.LearnerId, now, checked((int?)principal.Session.SchoolId));

}

