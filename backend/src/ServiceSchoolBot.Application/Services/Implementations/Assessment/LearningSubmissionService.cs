using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Services.Implementations.Assessment;

public sealed class LearningSubmissionService(IAssessmentRepository repository, IAssessmentAuthority authority,
    ICurriculumRepository curricula)
{
    public async Task<SubmissionView> CreateAsync(AssessmentPrincipalContext principal,
        CreateSubmissionRequest request, Guid key, CancellationToken ct = default)
    {
        Student(principal);
        if (request.MissionId <= 0 || request.CriteriaId <= 0 || request.ProjectRevisionId <= 0 ||
            request.Evidence?.Length > 8000) QuestionBankValidation.Invalid();
        if (request.Evidence is not null)
        {
            try { using var evidence = JsonDocument.Parse(request.Evidence); if (evidence.RootElement.ValueKind != JsonValueKind.Object) QuestionBankValidation.Invalid(); }
            catch (JsonException) { QuestionBankValidation.Invalid(); }
        }
        var result = await CommitAsync(principal, "create_submission", key, JsonSerializer.Serialize(request),
            async (scope, token) =>
            {
                var (curriculum, mission, criteria, mode) = await ContentAsync(scope, request.MissionId, request.CriteriaId, token);
                var project = await scope.GetLearningProjectAsync(request.ProjectRevisionId, token)
                    ?? throw new DomainRuleException(ErrorCodes.NotFound, "Project revision not found.");
                if (project.Project.SchoolId != principal.Session.SchoolId ||
                    project.Revision.SchoolId != principal.Session.SchoolId ||
                    project.Project.OwnerKind != "student" || project.Project.OwnerId != principal.LearnerId)
                    throw new DomainRuleException(ErrorCodes.NotFound, "Project revision not found.");
                if (project.Project.Mode != mode)
                    throw new DomainRuleException(ErrorCodes.StateConflict, "Project mode does not match the activity.");
                if (mission.Method == "automatic") LearningProgramEvaluator.Compile(mode, project.Revision.Content);
                var now = await scope.GetAssessmentTimeAsync(token);
                var candidates = await scope.ListCourseRunsAsync(checked((int)principal.Session.SchoolId!.Value),
                    curriculum.SchoolBotCurriculumId, token);
                var runs = new List<SchoolBotCourseRunRecord>();
                foreach (var run in candidates.Where(r => r.Lifecycle == "active" && r.EffectiveAt <= now))
                    if (await authority.CanReadRunAsync(principal, run, token)) runs.Add(run);
                if (runs.Count != 1)
                    throw new DomainRuleException("COURSE_RUN_NOT_RESOLVED", "Exactly one active course run is required.");
                var submission = new SchoolBotSubmissionRecord(0, runs[0].SchoolId, principal.LearnerId!.Value,
                    mission.SchoolBotMissionId, criteria.SchoolBotCriteriaId, request.ProjectRevisionId, now,
                    mission.Method == "teacher" ? "awaiting_teacher" : "queued", null, request.Evidence, now);
                var job = mission.Method == "automatic"
                    ? new SchoolBotGradingJobRecord(0, submission.SchoolId, 0, "queued", 0, null, null, now, null) : null;
                return await scope.WriteSubmissionAsync(submission, null, job,
                    Audit(principal, "Accepted", now, JsonSerializer.Serialize(new { course_run_id = runs[0].SchoolBotCourseRunId })), token);
            }, ct);
        return await ViewAsync(repository, result, ct);
    }

    public async Task<SubmissionView> GetAsync(AssessmentPrincipalContext principal, int id, CancellationToken ct = default)
    {
        var submission = await repository.GetSubmissionAsync(id)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Submission not found.");
        await AuthorizeSubmissionAsync(repository, principal, submission, ct);
        if (principal.Session.Channel == "desktop" && submission.State is "queued" or "grading")
            return await GradeAsync(principal, id, ct);
        return await ViewAsync(repository, submission, ct);
    }

    public async Task<IReadOnlyList<SubmissionView>> ListAsync(AssessmentPrincipalContext principal,
        int? learnerId, int? missionId, int limit = 50, CancellationToken ct = default)
    {
        Session(principal);
        if (limit is < 1 or > 100 || learnerId is <= 0 || missionId is <= 0) QuestionBankValidation.Invalid();
        if (principal.Session.Channel == "desktop")
        {
            Student(principal);
            if (learnerId is not null && learnerId != principal.LearnerId)
                throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Only the current learner can be selected.");
            learnerId = principal.LearnerId;
        }
        var result = new List<SubmissionView>();
        foreach (var submission in await repository.ListSubmissionsAsync(checked((int)principal.Session.SchoolId!.Value), learnerId, missionId, ct))
        {
            try { await AuthorizeSubmissionAsync(repository, principal, submission, ct); }
            catch (DomainRuleException ex) when (ex.Code is ErrorCodes.NotFound or ErrorCodes.ProfileNotAllowed) { continue; }
            result.Add(await ViewAsync(repository, submission, ct));
            if (result.Count == limit) break;
        }
        return result;
    }

    public async Task<SubmissionView> ReviewAsync(AssessmentPrincipalContext principal, int id,
        ReviewSubmissionRequest request, string? etag, Guid key, CancellationToken ct = default)
    {
        Teacher(principal);
        if (string.IsNullOrWhiteSpace(etag)) throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match is required.");
        if (request.Outcome is not ("passed" or "not_yet") || string.IsNullOrWhiteSpace(request.Feedback) ||
            request.Feedback.Length > 4000 || request.RuleResults is null || request.Reason?.Length > 1000)
            QuestionBankValidation.Invalid();
        var result = await CommitAsync(principal, "review_submission", key, JsonSerializer.Serialize(new { id, request, etag }),
            async (scope, token) =>
            {
                var submission = await scope.GetSubmissionAsync(id, token)
                    ?? throw new DomainRuleException(ErrorCodes.NotFound, "Submission not found.");
                await AuthorizeSubmissionAsync(scope, principal, submission, token);
                var (_, mission, criteria, _) = await ContentAsync(scope, submission.MissionId, submission.CriteriaId, token);
                if (mission.Method != "teacher") throw new DomainRuleException("ASSESSMENT_METHOD_MISMATCH", "Only teacher-assessed missions can be reviewed.");
                if (!OpaqueConcurrencyToken.Matches(etag, submission.UpdateDate))
                    throw new DomainRuleException(ErrorCodes.VersionConflict, "Submission changed.");
                if (submission.CurrentAssessmentId != request.SupersedesId ||
                    request.SupersedesId is not null && string.IsNullOrWhiteSpace(request.Reason))
                    throw new DomainRuleException(ErrorCodes.StateConflict, "A correction must supersede the current assessment and include a reason.");
                using var rules = JsonDocument.Parse(criteria.Rules);
                var expected = rules.RootElement.GetProperty("items").EnumerateArray().Select(r => r.GetProperty("id").GetGuid()).ToHashSet();
                if (request.RuleResults.Count != expected.Count || request.RuleResults.Select(r => r.RuleId).Distinct().Count() != expected.Count ||
                    request.RuleResults.Any(r => !expected.Contains(r.RuleId)) ||
                    request.Outcome == "passed" && request.RuleResults.Any(r => !r.Matched)) QuestionBankValidation.Invalid();
                var now = await scope.GetAssessmentTimeAsync(token);
                var history = await scope.ListSubmissionAssessmentsAsync(id, token);
                var assessment = new SchoolBotAssessmentRecord(0, submission.SchoolId, id, history.Count + 1,
                    "teacher", request.Outcome, null, null, JsonSerializer.Serialize(request.RuleResults), request.Feedback,
                    null, principal.EmployeeId, request.SupersedesId, now);
                var updated = submission with { State = "evaluated", UpdateDate = now > submission.UpdateDate ? now : submission.UpdateDate.AddTicks(10) };
                return await scope.WriteSubmissionAsync(updated, assessment, null,
                    Audit(principal, "Assessed", now, request.Reason ?? "Teacher assessment"), token);
            }, ct);
        return await ViewAsync(repository, result, ct);
    }

    private async Task AuthorizeSubmissionAsync(IAssessmentRepository scope, AssessmentPrincipalContext principal,
        SchoolBotSubmissionRecord submission, CancellationToken ct)
    {
        Session(principal);
        if (submission.SchoolId != principal.Session.SchoolId) throw new DomainRuleException(ErrorCodes.NotFound, "Submission not found.");
        var audits = await scope.ListAuditEventsByEntityAsync("SchoolBotSubmission", submission.Id, ct);
        var acceptance = audits.SingleOrDefault(a => a.Event == "Submission.Accepted")
            ?? throw new DomainRuleException("SOURCE_SCOPE_UNPROVEN", "Submission run evidence is unavailable.");
        using var reason = JsonDocument.Parse(acceptance.Reason!);
        var run = await scope.GetCourseRunByIdAsync(reason.RootElement.GetProperty("course_run_id").GetInt32(), ct)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Course run not found.");
        if (run.SchoolId != submission.SchoolId) throw new DomainRuleException(ErrorCodes.NotFound, "Submission not found.");
        if (principal.Session.Channel == "desktop")
        {
            Student(principal);
            if (submission.LearnerId == principal.LearnerId && await authority.CanReadRunAsync(principal, run, ct)) return;
        }
        else
        {
            Teacher(principal);
            if (await authority.CanReviewLearnerAsync(principal, run, submission.LearnerId, ct)) return;
        }
        throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Learner is outside the authorized course roster.");
    }

    private async Task<SchoolBotSubmissionRecord> CommitAsync(AssessmentPrincipalContext principal, string operation,
        Guid key, string payload, Func<ISchoolBotRepositoryScope, CancellationToken, Task<SchoolBotSubmissionRecord>> mutate, CancellationToken ct)
    {
        if (key == Guid.Empty) QuestionBankValidation.Invalid();
        if (repository is not IIdempotentSchoolBotCommit commit)
            throw new DomainRuleException(ErrorCodes.ServiceUnavailable, "Atomic learning storage is required.");
        var command = new IdempotencyCommand(checked((int?)principal.Session.SchoolId), principal.Session.PrincipalId!, operation,
            key, Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(payload))));
        var committed = await commit.CommitWithIdempotencyAsync(command,
            async (scope, resourceId, _, token) =>
            {
                var current = principal.Session.RecheckAsync is null ? principal.Session : await principal.Session.RecheckAsync(scope, token);
                Session(principal with { Session = current });
                if (current.PrincipalId != principal.Session.PrincipalId || current.SchoolId != principal.Session.SchoolId)
                    throw new DomainRuleException(ErrorCodes.Unauthorized, "Principal changed.");
                if (resourceId is { } id)
                    await AuthorizeSubmissionAsync(scope, principal with { Session = current },
                        await scope.GetSubmissionAsync(id, token) ?? throw new DomainRuleException(ErrorCodes.NotFound, "Submission not found."), token);
            }, async (scope, _, token) =>
            {
                var submission = await mutate(scope, token);
                return new MutationResult<SchoolBotSubmissionRecord>(submission, 200, submission.Id,
                    OpaqueConcurrencyToken.ToETag(submission.UpdateDate));
            }, ct);
        return committed.Result.Body;
    }

    private static async Task<SubmissionView> ViewAsync(IAssessmentRepository scope, SchoolBotSubmissionRecord submission, CancellationToken ct)
    {
        var project = await scope.GetLearningProjectAsync(submission.ProjectRevisionId, ct);
        return new(submission, (await scope.ListSubmissionAssessmentsAsync(submission.Id, ct)).SingleOrDefault(a => a.Id == submission.CurrentAssessmentId),
            project?.Revision.Content, project?.Project.Mode);
    }

    private static void Student(AssessmentPrincipalContext principal)
    {
        Session(principal);
        if (principal.Session.Channel != "desktop") throw new DomainRuleException(ErrorCodes.ChannelNotAllowed, "Desktop channel is required.");
        if (principal.ProfileKind != "student" || principal.LearnerId is null or <= 0)
            throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Student profile is required.");
    }

    private static void Teacher(AssessmentPrincipalContext principal)
    {
        Session(principal);
        if (principal.Session.Channel != "school_web" || principal.ProfileKind != "employee" || principal.EmployeeId is null)
            throw new DomainRuleException(ErrorCodes.ChannelNotAllowed, "School teacher channel is required.");
    }

    private static void Session(AssessmentPrincipalContext principal)
    {
        if (!principal.Session.IsAuthConfigured) throw new DomainRuleException(ErrorCodes.AuthNotConfigured, "Authentication is unavailable.");
        if (principal.Session.IsSessionExpired || string.IsNullOrWhiteSpace(principal.Session.PrincipalId) || principal.Session.SchoolId is null)
            throw new DomainRuleException(ErrorCodes.Unauthorized, "An authenticated school profile is required.");
        if (!principal.Session.HasActiveSchoolLicense) throw new DomainRuleException(ErrorCodes.LicenseRequired, "An active license is required.");
    }

    private static SchoolBotAuditEventRecord Audit(AssessmentPrincipalContext principal, string action, DateTimeOffset now, string reason) =>
        new(0, principal.Session.PrincipalId!, "Submission." + action, "SchoolBotSubmission", 0, Guid.NewGuid(), reason,
            false, principal.EmployeeId ?? principal.LearnerId, now, principal.EmployeeId ?? principal.LearnerId, now, checked((int?)principal.Session.SchoolId));

    public async Task<JsonElement> GetRunsAsync(AssessmentPrincipalContext principal, int curriculumId, CancellationToken ct = default)
    {
        Session(principal);
        var visible = new List<object>();
        foreach (var run in await repository.ListCourseRunsAsync(checked((int)principal.Session.SchoolId!.Value), curriculumId, ct))
            if (await authority.CanReadRunAsync(principal, run, ct))
                visible.Add(new { id = run.SchoolBotCourseRunId, n_year = run.NYear, n_term = run.NTerm, classroom_id = run.ClassroomId, lifecycle = run.Lifecycle });
        return JsonSerializer.SerializeToElement(new { items = visible });
    }
    public async Task<JsonElement> GetCurriculumAsync(AssessmentPrincipalContext principal, int id, CancellationToken ct = default)
    {
        Session(principal);
        var curriculum = await curricula.GetCurriculumByIdAsync(id, ct)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Curriculum not found.");
        if (curriculum.IsDelete || curriculum.Lifecycle != "published" || curriculum.SchoolId is not null)
            throw new DomainRuleException(ErrorCodes.NotFound, "Published curriculum not found.");
        var lessons = new List<object>();
        foreach (var lesson in await curricula.ListLessonsByCurriculumIdAsync(id, ct))
        {
            var missions = new List<object>();
            var storedMissions = await curricula.ListMissionsByLessonIdAsync(lesson.SchoolBotLessonId, ct);
            foreach (var mission in storedMissions)
            {
                var criteria = (await curricula.ListCriteriaByMissionIdAsync(mission.SchoolBotMissionId, ct))
                    .Where(c => c.ApprovedAt is not null && c.ApprovedBy is not null && !c.IsDelete)
                    .OrderByDescending(c => c.Revision).FirstOrDefault();
                if (criteria is null) continue;
                missions.Add(new { id = mission.SchoolBotMissionId, mission.Title, mission.Instructions, mission.Method,
                    criteria = new { id = criteria.SchoolBotCriteriaId, rules = JsonSerializer.Deserialize<JsonElement>(criteria.Rules) } });
            }
            var activities = (await curricula.ListActivitiesByLessonIdAsync(lesson.SchoolBotLessonId, ct))
                .Select(a => new { id = a.SchoolBotActivityId, a.Title, a.Type, a.Mode,
                    config = LearningConfig(a, storedMissions) }).ToArray();
            lessons.Add(new { id = lesson.SchoolBotLessonId, lesson.Title, lesson.Instructions, missions, activities });
        }
        return JsonSerializer.SerializeToElement(new { id, curriculum.Title, curriculum.Revision, lessons },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
    }

    private static JsonElement LearningConfig(SchoolBotActivityRecord activity, IReadOnlyList<SchoolBotMissionRecord> missions)
    {
        if (activity.Type != "project") return JsonSerializer.Deserialize<JsonElement>(activity.Config);
        var config = JsonNode.Parse(activity.Config)!.AsObject();
        config["mission_id"] = ProjectActivityReference.ResolveMissionId(activity.Config, missions);
        return JsonSerializer.SerializeToElement(config);
    }


    private static async Task<(SchoolBotCurriculumRecord Curriculum, SchoolBotMissionRecord Mission,
        SchoolBotCriteriaRecord Criteria, string Mode)> ContentAsync(ICurriculumRepository scope, int missionId,
        int criteriaId, CancellationToken ct)
    {
        foreach (var curriculum in await scope.ListCurriculaAsync(lifecycle: "published", ct: ct))
        {
            if (curriculum.SchoolId is not null || curriculum.IsDelete) continue;
            foreach (var lesson in await scope.ListLessonsByCurriculumIdAsync(curriculum.SchoolBotCurriculumId, ct))
            {
                var lessonMissions = await scope.ListMissionsByLessonIdAsync(lesson.SchoolBotLessonId, ct);
                var mission = lessonMissions
                    .SingleOrDefault(m => m.SchoolBotMissionId == missionId && !m.IsDelete && m.SchoolId is null);
                if (mission is null) continue;
                var criteria = (await scope.ListCriteriaByMissionIdAsync(missionId, ct))
                    .SingleOrDefault(c => c.SchoolBotCriteriaId == criteriaId && !c.IsDelete && c.SchoolId is null)
                    ?? throw new DomainRuleException(ErrorCodes.NotFound, "Criteria not found for mission.");
                if (criteria.ApprovedAt is null || string.IsNullOrWhiteSpace(criteria.ApprovedBy))
                    throw new DomainRuleException(ErrorCodes.CriteriaNotApproved, "Approved criteria are required.");
                foreach (var activity in await scope.ListActivitiesByLessonIdAsync(lesson.SchoolBotLessonId, ct))
                {
                    if (activity.IsDelete || activity.SchoolId is not null || activity.Type != "project") continue;
                    if (ProjectActivityReference.ResolveMissionId(activity.Config, lessonMissions) == missionId)
                        return (curriculum, mission, criteria, activity.Mode!);
                }
                throw new DomainRuleException(ErrorCodes.StateConflict, "Mission is not linked to a published project activity.");
            }
        }
        throw new DomainRuleException(ErrorCodes.NotFound, "Published mission not found.");
    }

    public async Task<SubmissionView> GradeAsync(AssessmentPrincipalContext principal, int id, CancellationToken ct = default)
    {
        Student(principal);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"grade-submission:{id}"));
        var saved = await CommitAsync(principal, "grade_submission", new Guid(bytes.AsSpan(0, 16)), id.ToString(),
            async (scope, token) =>
            {
                var submission = await scope.GetSubmissionAsync(id, token)
                    ?? throw new DomainRuleException(ErrorCodes.NotFound, "Submission not found.");
                await AuthorizeSubmissionAsync(scope, principal, submission, token);
                return await EvaluateAsync(scope, submission, token);
            }, ct);
        return await ViewAsync(repository, saved, ct);
    }


    // The bounded evaluator runs inside the atomic commit, so a crash rolls back both job and result.
    // No worker can own a lease after releasing the transaction or evaluate a different revision.
    private static async Task<SchoolBotSubmissionRecord> EvaluateAsync(ISchoolBotRepositoryScope scope,
        SchoolBotSubmissionRecord submission, CancellationToken token)
    {
                if (submission.State == "evaluated") return submission;
                if (submission.State is not ("queued" or "grading"))
                    throw new DomainRuleException(ErrorCodes.StateConflict, "Submission cannot be automatically graded.");
                var (_, mission, criteria, mode) = await ContentAsync(scope, submission.MissionId, submission.CriteriaId, token);
                if (mission.Method != "automatic")
                    throw new DomainRuleException("ASSESSMENT_METHOD_MISMATCH", "Teacher review is required.");
                var project = await scope.GetLearningProjectAsync(submission.ProjectRevisionId, token)
                    ?? throw new DomainRuleException(ErrorCodes.NotFound, "Project revision not found.");
                if (project.Project.OwnerKind != "student" || project.Project.OwnerId != submission.LearnerId ||
                    project.Project.SchoolId != submission.SchoolId || project.Revision.SchoolId != submission.SchoolId ||
                    project.Project.Mode != mode) throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Project scope changed.");
                var evaluation = LearningProgramEvaluator.Evaluate(LearningProgramEvaluator.Compile(mode, project.Revision.Content), criteria.Rules);
                var now = await scope.GetAssessmentTimeAsync(token);
                var id = submission.Id;
                var history = await scope.ListSubmissionAssessmentsAsync(id, token);
                var assessment = new SchoolBotAssessmentRecord(0, submission.SchoolId, id, history.Count + 1, "automatic",
                    evaluation.Outcome, null, null, JsonSerializer.Serialize(evaluation.Rules), "", evaluation.Version,
                    null, submission.CurrentAssessmentId, now);
                var job = await scope.GetGradingJobAsync(id, token)
                    ?? throw new DomainRuleException(ErrorCodes.StateConflict, "Durable grading job is missing.");
                return await scope.WriteSubmissionAsync(submission with { State = "evaluated", UpdateDate = now }, assessment,
                    job with { State = "completed", Attempts = job.Attempts + 1, LeaseOwner = null, LeaseUntil = null },
                    new(0, "service:program-grader", "Submission.Evaluated", "SchoolBotSubmission", id, Guid.NewGuid(),
                        evaluation.Version, false, null, now, null, now, submission.SchoolId), token);
    }

    public async Task<int> SweepAsync(int limit = 50, CancellationToken ct = default)
    {
        if (repository is not IIdempotentSchoolBotCommit commit)
            throw new DomainRuleException(ErrorCodes.ServiceUnavailable, "Atomic learning storage is required.");
        var completed = 0;
        foreach (var candidate in await repository.ListOpenGradingJobsAsync(Math.Clamp(limit, 1, 100), ct))
        {
            var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes($"grading-job:{candidate.Id}:{candidate.Attempts}")));
            var result = await commit.CommitWithIdempotencyAsync(new(candidate.SchoolId, "service:program-grader",
                "run_grading_job", new Guid(Convert.FromHexString(hash).AsSpan(0, 16)), hash),
                (_, _, _, _) => Task.CompletedTask, async (scope, _, token) =>
                {
                    var submission = await scope.GetSubmissionAsync(candidate.SubmissionId, token)
                        ?? throw new DomainRuleException(ErrorCodes.NotFound, "Submission not found.");
                    var job = await scope.GetGradingJobAsync(submission.Id, token)
                        ?? throw new DomainRuleException(ErrorCodes.NotFound, "Grading job not found.");
                    var now = await scope.GetAssessmentTimeAsync(token);
                    if (job.State != "queued" || job.Attempts != candidate.Attempts || job.NextAttemptAt > now)
                        return new MutationResult<SchoolBotSubmissionRecord>(submission, 200, submission.Id, null);
                    try
                    {
                        var evaluated = await EvaluateAsync(scope, submission, token);
                        return new MutationResult<SchoolBotSubmissionRecord>(evaluated, 200, evaluated.Id, null);
                    }
                    catch (DomainRuleException ex)
                    {
                        var attempts = job.Attempts + 1;
                        var failed = await scope.WriteSubmissionAsync(submission, null, job with
                        {
                            Attempts = attempts, State = attempts >= 5 ? "failed" : "queued", LastErrorCode = ex.Code,
                            NextAttemptAt = now.AddSeconds(Math.Min(3600, 30 * (1 << attempts))), LeaseOwner = null, LeaseUntil = null
                        }, new(0, "service:program-grader", "Submission.GradingDeferred", "SchoolBotSubmission", submission.Id,
                            Guid.NewGuid(), ex.Code, false, null, now, null, now, submission.SchoolId), token);
                        return new MutationResult<SchoolBotSubmissionRecord>(failed, 200, failed.Id, null);
                    }
                }, ct);
            if (result.Result.Body.State == "evaluated") completed++;
        }
        return completed;
    }
}

