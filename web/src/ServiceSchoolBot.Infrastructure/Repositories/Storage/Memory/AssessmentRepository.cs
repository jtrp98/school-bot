using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Projects;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed class AssessmentRepository : IAssessmentRepository, IIdempotentSchoolBotCommit
{
    public Task<SchoolBotCurriculumRecord> ApproveLearningCriteriaAsync(int curriculumId, string actor, DateTimeOffset now,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default) => TeacherMutationAsync(audit, snapshot =>
    {
        var curriculum = snapshot.Curricula.Single(c => c.SchoolBotCurriculumId == curriculumId && c.SchoolId == null && !c.IsDelete && c.Lifecycle == "draft");
        var lessonIds = snapshot.Lessons.Where(l => l.CurriculumId == curriculumId && !l.IsDelete).Select(l => l.SchoolBotLessonId).ToHashSet();
        var missionIds = snapshot.Missions.Where(m => lessonIds.Contains(m.LessonId) && !m.IsDelete).Select(m => m.SchoolBotMissionId).ToHashSet();
        var saved = curriculum with { UpdateDate = now };
        return (snapshot with { Curricula = snapshot.Curricula.Select(c => c.SchoolBotCurriculumId == curriculumId ? saved : c).ToArray(),
            Criteria = snapshot.Criteria.Select(c => missionIds.Contains(c.MissionId) && !c.IsDelete && c.SchoolId == null ? c with { ApprovedBy = actor, ApprovedAt = now, UpdateDate = now } : c).ToArray() }, saved, curriculumId);
    }, ct);
    public async Task<IReadOnlyList<SchoolBotLevelRecord>> ListLevelsAsync(CancellationToken ct = default) =>
        (await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).Levels ?? [];
    public async Task<SchoolBotLevelPromotionPolicyRecord?> GetPromotionPolicyAsync(int levelId, CancellationToken ct = default) =>
        ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).PromotionPolicies ?? [])
            .Where(p => p.LevelId == levelId).OrderByDescending(p => p.Revision).FirstOrDefault();
    public async Task<IReadOnlyList<SchoolBotLevelProposalRecord>> ListLevelProposalsAsync(int courseRunId, CancellationToken ct = default) =>
        ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).LevelProposals ?? []).Where(p => p.CourseRunId == courseRunId).ToArray();
    public async Task<SchoolBotLevelProposalRecord?> GetLevelProposalAsync(int id, CancellationToken ct = default) =>
        ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).LevelProposals ?? []).SingleOrDefault(p => p.Id == id);
    public Task<SchoolBotLevelProposalRecord> WriteLevelProposalAsync(SchoolBotLevelProposalRecord proposal,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default) => TeacherMutationAsync(audit, snapshot =>
    {
        var rows = snapshot.LevelProposals ?? [];
        var saved = proposal.Id == 0 ? proposal with { Id = rows.Select(p => p.Id).DefaultIfEmpty().Max() + 1 } : proposal;
        return (snapshot with { LevelProposals = [.. rows.Where(p => p.Id != saved.Id), saved] }, saved, saved.Id);
    }, ct);
    public async Task<SchoolBotGradingJobRecord?> GetGradingJobAsync(int submissionId, CancellationToken ct = default) =>
        ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).GradingJobs ?? []).SingleOrDefault(j => j.SubmissionId == submissionId);
    public async Task<IReadOnlyList<SchoolBotAttemptRecord>> ListExpirableAttemptsAsync(int limit, CancellationToken ct = default) =>
        ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).Attempts ?? [])
            .Where(a => !a.IsDelete && (a.State == "started" && a.DeadlineAt <= state.clock.GetUtcNow() || a.State == "expired" && a.EarnedScore is null))
            .OrderBy(a => a.SchoolBotAttemptId).Take(Math.Clamp(limit, 1, 100)).ToArray();
    private readonly SchoolBotUnitOfWork unitOfWork;
    private readonly SchoolBotRepositoryState state;

    public AssessmentRepository(SchoolBotUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
        state = unitOfWork.State;
    }

    public Task<IdempotentCommitResult<T>> CommitWithIdempotencyAsync<T>(
        IdempotencyCommand command,
        Func<ISchoolBotRepositoryScope, int?, DateTimeOffset, CancellationToken, Task> recheckAccess,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<MutationResult<T>>> mutate,
        CancellationToken ct = default) =>
        unitOfWork.CommitWithIdempotencyAsync(command, recheckAccess, mutate, ct);

    public async Task<SchoolBotQuestionBankRecord?> GetQuestionBankByIdAsync(int id, CancellationToken ct = default)
    {
        AssessmentId(id);
        return ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).QuestionBanks ?? [])
            .SingleOrDefault(row => row.SchoolBotQuestionBankId == id && !row.IsDelete);
    }

    public async Task<IReadOnlyList<SchoolBotQuestionBankRecord>> ListQuestionBanksAsync(
        Guid? bankKey = null, string? lifecycle = null, CancellationToken ct = default)
    {
        if (bankKey == Guid.Empty || lifecycle is not (null or "draft" or "published" or "retired"))
            QuestionBankValidation.Invalid();
        return ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).QuestionBanks ?? [])
            .Where(row => !row.IsDelete && (bankKey is null || row.BankKey == bankKey) &&
                (lifecycle is null || row.Lifecycle == lifecycle))
            .OrderBy(row => row.BankKey).ThenBy(row => row.Revision).ToArray();
    }

    public async Task<SchoolBotCourseRunRecord?> GetCourseRunByIdAsync(int id, CancellationToken ct = default)
    {
        AssessmentId(id);
        return ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).CourseRuns ?? [])
            .SingleOrDefault(row => row.SchoolBotCourseRunId == id && !row.IsDelete);
    }

    public async Task<SchoolBotCourseRunActivityRecord?> GetCourseRunActivityAsync(
        int courseRunId, int activityId, CancellationToken ct = default)
    {
        AssessmentId(courseRunId);
        AssessmentId(activityId);
        return ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).CourseRunActivities ?? [])
            .SingleOrDefault(row => row.CourseRunId == courseRunId && row.ActivityId == activityId && !row.IsDelete);
    }

    public async Task<SchoolBotAttemptRecord?> GetAttemptByIdAsync(int id, CancellationToken ct = default)
    {
        AssessmentId(id);
        return ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).Attempts ?? [])
            .SingleOrDefault(row => row.SchoolBotAttemptId == id && !row.IsDelete);
    }

    public async Task<IReadOnlyList<SchoolBotAttemptRecord>> ListAttemptsAsync(
        int courseRunId, int? activityId, int learnerId, CancellationToken ct = default)
    {
        AssessmentId(courseRunId);
        if (activityId is not null) AssessmentId(activityId.Value);
        return ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).Attempts ?? [])
            .Where(row => !row.IsDelete && row.CourseRunId == courseRunId && row.LearnerId == learnerId &&
                (activityId is null || row.ActivityId == activityId))
            .OrderByDescending(row => row.StartedAt).ThenByDescending(row => row.SchoolBotAttemptId).ToArray();
    }

    public Task<SchoolBotQuestionBankRecord> CreateQuestionBankAsync(
        SchoolBotQuestionBankRecord record, SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        RequireAssessmentTransaction();
        QuestionBankService.ValidateStoredNewBank(record);
        QuestionBankService.ValidateStoredBankAudit(audit, "QuestionBank.Created");
        return state.CurriculumStore.MutateCurriculumUnitAsync((snapshot, token) =>
        {
            var banks = snapshot.QuestionBanks ?? [];
            if (banks.Any(row => !row.IsDelete && row.BankKey == record.BankKey && row.Revision == record.Revision))
                throw new DomainRuleException(ErrorCodes.StateConflict, "Question bank revision already exists.");
            var now = state.clock.GetUtcNow();
            var created = record with
            {
                SchoolBotQuestionBankId = checked(banks.Select(row => row.SchoolBotQuestionBankId).DefaultIfEmpty().Max() + 1),
                CreateBy = audit.CreateBy,
                UpdateBy = audit.CreateBy,
                CreateDate = now,
                UpdateDate = now
            };
            return Task.FromResult(BankMutation(snapshot, [.. banks, created], created, audit, now));
        }, ct);
    }

    public Task<SchoolBotQuestionBankRecord> UpdateQuestionBankDraftAsync(
        int id, string title, IReadOnlyList<QuestionBankQuestion> questions, string sourceRef,
        string expectedETag, SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        RequireAssessmentTransaction();
        AssessmentId(id);
        QuestionBankValidation.Validate(title, questions, sourceRef);
        QuestionBankService.ValidateStoredBankAudit(audit, "QuestionBank.Updated");
        return ChangeBankAsync(id, expectedETag, audit, (existing, now) => existing with
        {
            Title = title,
            Questions = questions,
            SourceRef = sourceRef,
            UpdateBy = audit.CreateBy,
            UpdateDate = now
        }, ct);
    }

    public Task<SchoolBotQuestionBankRecord> PublishQuestionBankAsync(
        int id, string expectedETag, SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        RequireAssessmentTransaction();
        AssessmentId(id);
        QuestionBankService.ValidateStoredBankAudit(audit, "QuestionBank.Published");
        return ChangeBankAsync(id, expectedETag, audit, (existing, now) =>
        {
            return QuestionBankService.PrepareStoredBankPublication(existing, audit, now);
        }, ct);
    }

    private Task<SchoolBotQuestionBankRecord> ChangeBankAsync(
        int id, string expectedETag, SchoolBotAuditEventRecord audit,
        Func<SchoolBotQuestionBankRecord, DateTimeOffset, SchoolBotQuestionBankRecord> change, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(expectedETag))
            throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match is required.");
        return state.CurriculumStore.MutateCurriculumUnitAsync((snapshot, token) =>
        {
            var banks = snapshot.QuestionBanks ?? [];
            var existing = banks.SingleOrDefault(row => row.SchoolBotQuestionBankId == id && !row.IsDelete)
                ?? throw new DomainRuleException(ErrorCodes.NotFound, "Question bank not found.");
            QuestionBankService.ValidateStoredBankChange(existing, expectedETag, audit, snapshot.AuditEvents, id);
            var now = state.clock.GetUtcNow();
            if (existing.UpdateDate is { } previous && now <= previous) now = previous.AddTicks(1);
            var updated = change(existing, now);
            return Task.FromResult(BankMutation(snapshot,
                banks.Select(row => row.SchoolBotQuestionBankId == id ? updated : row).ToArray(), updated, audit, now));
        }, ct);
    }

    private static CurriculumStoreMutation<SchoolBotQuestionBankRecord> BankMutation(
        CurriculumStoreSnapshot snapshot, IReadOnlyList<SchoolBotQuestionBankRecord> banks,
        SchoolBotQuestionBankRecord bank, SchoolBotAuditEventRecord audit, DateTimeOffset now)
    {
        var entry = audit with
        {
            SchoolBotAuditEventId = checked(snapshot.AuditEvents.Select(row => row.SchoolBotAuditEventId)
                .DefaultIfEmpty().Max() + 1),
            EntityId = bank.SchoolBotQuestionBankId,
            CreateDate = now,
            UpdateDate = now
        };
        return new(snapshot with { QuestionBanks = banks, AuditEvents = [.. snapshot.AuditEvents, entry] }, bank);
    }

    public async Task<IReadOnlyList<SchoolBotAuditEventRecord>> ListAuditEventsByEntityAsync(string entityType, int entityId, CancellationToken ct = default)
    {
        await state.curriculumSyncLock.WaitAsync(ct);
        try
        {
            await state.ReloadCurriculaUnderLockAsync(ct);
            return state.auditEvents
                .Where(a => !a.IsDelete &&
                            string.Equals(a.EntityType, entityType, StringComparison.OrdinalIgnoreCase) &&
                            a.EntityId == entityId)
                .OrderBy(a => a.CreateDate)
                .ToArray();
        }
        finally { state.curriculumSyncLock.Release(); }
    }

    private void RequireAssessmentTransaction()
    {
        if (!state.isCurriculumTransaction)
            throw new InvalidOperationException("Question bank writes require an idempotent transaction facade.");
    }

    private static void AssessmentId(int id)
    {
        if (id <= 0) QuestionBankValidation.Invalid();
    }

    public Task<DateTimeOffset> GetAssessmentTimeAsync(CancellationToken ct = default) =>
        Task.FromResult(state.clock.GetUtcNow());
    public Task<SchoolBotAttemptRecord> WriteAttemptAsync(SchoolBotAttemptRecord record,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        RequireAssessmentTransaction();
        return state.CurriculumStore.MutateCurriculumUnitAsync((snapshot, _) =>
        {
            var attempts = snapshot.Attempts ?? [];
            var saved = record.SchoolBotAttemptId == 0 ? record with
            {
                SchoolBotAttemptId = checked(attempts.Select(r => r.SchoolBotAttemptId).DefaultIfEmpty().Max() + 1)
            } : record;
            var rows = attempts.Where(r => r.SchoolBotAttemptId != saved.SchoolBotAttemptId).Append(saved).ToArray();
            var entry = audit with
            {
                SchoolBotAuditEventId = checked(snapshot.AuditEvents.Select(r => r.SchoolBotAuditEventId).DefaultIfEmpty().Max() + 1),
                EntityId = saved.SchoolBotAttemptId
            };
            return Task.FromResult(new CurriculumStoreMutation<SchoolBotAttemptRecord>(
                snapshot with { Attempts = rows, AuditEvents = [.. snapshot.AuditEvents, entry] }, saved));
        }, ct);
    }

    public async Task<IReadOnlyList<SchoolBotGradingJobRecord>> ListOpenGradingJobsAsync(int limit, CancellationToken ct = default) =>
        ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).GradingJobs ?? [])
            .Where(j => j.State == "queued" && j.NextAttemptAt <= state.clock.GetUtcNow()).OrderBy(j => j.Id).Take(limit).ToArray();
    public async Task<(SchoolBotProjectRecord Project, SchoolBotProjectRevisionRecord Revision)?> GetLearningProjectAsync(
        int revisionId, CancellationToken ct = default)
    {
        var revision = (await state.backingStore.LoadProjectRevisionsAsync(ct))
            .SingleOrDefault(r => r.SchoolBotProjectRevisionId == revisionId && !r.IsDelete);
        if (revision is null) return null;
        var project = (await state.backingStore.LoadProjectsAsync(ct))
            .SingleOrDefault(p => p.SchoolBotProjectId == revision.ProjectId && !p.IsDelete);
        return project is null ? null : (project, revision);
    }

    public async Task<IReadOnlyList<SchoolBotCourseRunRecord>> ListCourseRunsAsync(int schoolId, int curriculumId,
        CancellationToken ct = default) => ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).CourseRuns ?? [])
        .Where(r => !r.IsDelete && r.SchoolId == schoolId && (curriculumId == 0 || r.CurriculumId == curriculumId)).ToArray();

    public async Task<SchoolBotSubmissionRecord?> GetSubmissionAsync(int id, CancellationToken ct = default) =>
        ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).Submissions ?? []).SingleOrDefault(s => s.Id == id);

    public async Task<IReadOnlyList<SchoolBotSubmissionRecord>> ListSubmissionsAsync(int schoolId, int? learnerId,
        int? missionId, CancellationToken ct = default) =>
        ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).Submissions ?? [])
            .Where(s => s.SchoolId == schoolId && (learnerId is null || s.LearnerId == learnerId) &&
                (missionId is null || s.MissionId == missionId)).OrderByDescending(s => s.SubmittedAt)
            .ThenByDescending(s => s.Id).ToArray();

    public async Task<IReadOnlyList<SchoolBotAssessmentRecord>> ListSubmissionAssessmentsAsync(int submissionId,
        CancellationToken ct = default) => ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).Assessments ?? [])
        .Where(a => a.SubmissionId == submissionId).OrderBy(a => a.Sequence).ToArray();

    public Task<SchoolBotSubmissionRecord> WriteSubmissionAsync(SchoolBotSubmissionRecord submission,
        SchoolBotAssessmentRecord? assessment, SchoolBotGradingJobRecord? job, SchoolBotAuditEventRecord audit,
        CancellationToken ct = default)
    {
        RequireAssessmentTransaction();
        return state.CurriculumStore.MutateCurriculumUnitAsync((snapshot, _) =>
        {
            var submissions = snapshot.Submissions ?? [];
            var assessments = snapshot.Assessments ?? [];
            var jobs = snapshot.GradingJobs ?? [];
            var saved = submission.Id == 0 ? submission with { Id = checked(submissions.Select(s => s.Id).DefaultIfEmpty().Max() + 1) } : submission;
            if (assessment is not null)
            {
                var result = assessment with
                {
                    Id = checked(assessments.Select(a => a.Id).DefaultIfEmpty().Max() + 1), SubmissionId = saved.Id
                };
                saved = saved with { CurrentAssessmentId = result.Id };
                assessments = [.. assessments, result];
            }
            if (job is not null)
            {
                var entry = job with { Id = job.Id == 0 ? checked(jobs.Select(j => j.Id).DefaultIfEmpty().Max() + 1) : job.Id, SubmissionId = saved.Id };
                jobs = [.. jobs.Where(j => j.Id != entry.Id), entry];
            }
            var auditRow = audit with
            {
                SchoolBotAuditEventId = checked(snapshot.AuditEvents.Select(a => a.SchoolBotAuditEventId).DefaultIfEmpty().Max() + 1),
                EntityId = saved.Id
            };
            return Task.FromResult(new CurriculumStoreMutation<SchoolBotSubmissionRecord>(snapshot with
            {
                Submissions = [.. submissions.Where(s => s.Id != saved.Id), saved], Assessments = assessments,
                GradingJobs = jobs, AuditEvents = [.. snapshot.AuditEvents, auditRow]
            }, saved));
        }, ct);
    }

    public Task<SchoolBotCourseRunActivityRecord> WriteActivityPolicyAsync(SchoolBotCourseRunActivityRecord policy,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default) => TeacherMutationAsync(audit, snapshot =>
    {
        var rows = snapshot.CourseRunActivities ?? [];
        var saved = policy.SchoolBotCourseRunActivityId == 0 ? policy with
        { SchoolBotCourseRunActivityId = rows.Select(r => r.SchoolBotCourseRunActivityId).DefaultIfEmpty().Max() + 1 } : policy;
        return (snapshot with { CourseRunActivities = [.. rows.Where(r => r.SchoolBotCourseRunActivityId != saved.SchoolBotCourseRunActivityId), saved] }, saved, saved.SchoolBotCourseRunActivityId);
    }, ct);

    public async Task<IReadOnlyList<SchoolBotRetakeGrantRecord>> ListRetakeGrantsAsync(int courseRunId,
        int activityId, int learnerId, CancellationToken ct = default) =>
        ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).RetakeGrants ?? [])
        .Where(g => g.CourseRunId == courseRunId && g.ActivityId == activityId && g.LearnerId == learnerId).OrderBy(g => g.Id).ToArray();

    public Task<SchoolBotRetakeGrantRecord> WriteRetakeGrantAsync(SchoolBotRetakeGrantRecord grant,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default) => TeacherMutationAsync(audit, snapshot =>
    {
        var rows = snapshot.RetakeGrants ?? [];
        var saved = grant.Id == 0 ? grant with { Id = rows.Select(g => g.Id).DefaultIfEmpty().Max() + 1 } : grant;
        return (snapshot with { RetakeGrants = [.. rows.Where(g => g.Id != saved.Id), saved] }, saved, saved.Id);
    }, ct);

    public async Task<SchoolBotScoringPolicyRecord?> GetScoringPolicyAsync(int id, CancellationToken ct = default) =>
        ((await state.CurriculumStore.LoadCurriculumUnitAsync(ct)).ScoringPolicies ?? []).SingleOrDefault(p => p.Id == id);

    public Task<SchoolBotScoringPolicyRecord> WriteScoringPolicyAsync(SchoolBotScoringPolicyRecord policy,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default) => TeacherMutationAsync(audit, snapshot =>
    {
        var rows = snapshot.ScoringPolicies ?? [];
        var saved = policy with { Id = rows.Select(p => p.Id).DefaultIfEmpty().Max() + 1 };
        return (snapshot with { ScoringPolicies = [.. rows, saved] }, saved, saved.Id);
    }, ct);

    public Task<SchoolBotCourseRunRecord> WriteCourseRunAsync(SchoolBotCourseRunRecord run,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default) => TeacherMutationAsync(audit, snapshot =>
    {
        var rows = snapshot.CourseRuns ?? [];
        var saved = run.SchoolBotCourseRunId == 0 ? run with
        { SchoolBotCourseRunId = rows.Select(r => r.SchoolBotCourseRunId).DefaultIfEmpty().Max() + 1 } : run;
        return (snapshot with { CourseRuns = [.. rows.Where(r => r.SchoolBotCourseRunId != saved.SchoolBotCourseRunId), saved] }, saved, saved.SchoolBotCourseRunId);
    }, ct);

    private Task<T> TeacherMutationAsync<T>(SchoolBotAuditEventRecord audit,
        Func<CurriculumStoreSnapshot, (CurriculumStoreSnapshot Snapshot, T Saved, int Id)> mutate, CancellationToken ct)
    {
        RequireAssessmentTransaction();
        return state.CurriculumStore.MutateCurriculumUnitAsync((snapshot, _) =>
        {
            var changed = mutate(snapshot);
            var entry = audit with { SchoolBotAuditEventId = snapshot.AuditEvents.Select(a => a.SchoolBotAuditEventId).DefaultIfEmpty().Max() + 1, EntityId = changed.Id };
            return Task.FromResult(new CurriculumStoreMutation<T>(changed.Snapshot with { AuditEvents = [.. snapshot.AuditEvents, entry] }, changed.Saved));
        }, ct);
    }

}


