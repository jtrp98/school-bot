using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;
using System.Text.Json;
using ServiceSchoolBot.Domain.Projects;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed class AssessmentRepository(SchoolBotDbContext db, SchoolBotUnitOfWork unitOfWork) : IAssessmentRepository, IIdempotentSchoolBotCommit
{
    public async Task<SchoolBotCurriculumRecord> ApproveLearningCriteriaAsync(int curriculumId, string actor, DateTimeOffset now,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        unitOfWork.RequireIdempotentCommit();
        var curriculum = await db.SchoolBotCurricula.SingleAsync(c => c.SchoolBotCurriculumId == curriculumId && c.SchoolId == null && !c.IsDelete && c.Lifecycle == "draft", ct);
        var criteria = await (from c in db.SchoolBotCriteria join m in db.SchoolBotMissions on c.MissionId equals m.SchoolBotMissionId
            join l in db.SchoolBotLessons on m.LessonId equals l.SchoolBotLessonId
            where l.CurriculumId == curriculumId && !l.IsDelete && !m.IsDelete && !c.IsDelete && c.SchoolId == null select c).ToArrayAsync(ct);
        foreach (var criterion in criteria) { criterion.ApprovedBy = actor; criterion.ApprovedAt = now.UtcDateTime; criterion.UpdateDate = now.UtcDateTime; }
        curriculum.UpdateDate = now.UtcDateTime;
        await unitOfWork.AddAuditAsync(audit, curriculumId, ct);
        return (await unitOfWork.Curriculum.GetCurriculumByIdAsync(curriculumId, ct))!;
    }
    public async Task<IReadOnlyList<SchoolBotLevelRecord>> ListLevelsAsync(CancellationToken ct = default) =>
        (await db.SchoolBotLevels.AsNoTracking().Where(l => !l.IsDelete && l.SchoolId == null)
            .OrderBy(l => l.Ordinal).ToArrayAsync(ct)).Select(l => new SchoolBotLevelRecord(l.SchoolBotLevelId, l.Ordinal, l.Title)).ToArray();

    public async Task<SchoolBotLevelPromotionPolicyRecord?> GetPromotionPolicyAsync(int levelId, CancellationToken ct = default)
    {
        var p = await db.SchoolBotLevelPromotionPolicies.AsNoTracking().Where(p => !p.IsDelete && p.SchoolId == null && p.LevelId == levelId)
            .OrderByDescending(p => p.Revision).FirstOrDefaultAsync(ct);
        return p is null ? null : new(p.SchoolBotLevelPromotionPolicyId, p.LevelId, p.Revision, p.Threshold);
    }

    public async Task<IReadOnlyList<SchoolBotLevelProposalRecord>> ListLevelProposalsAsync(int courseRunId, CancellationToken ct = default) =>
        (await db.SchoolBotLevelProposals.AsNoTracking().Where(p => !p.IsDelete && p.CourseRunId == courseRunId).ToArrayAsync(ct)).Select(From).ToArray();

    public async Task<SchoolBotLevelProposalRecord?> GetLevelProposalAsync(int id, CancellationToken ct = default)
    {
        var row = await db.SchoolBotLevelProposals.AsNoTracking().SingleOrDefaultAsync(p => !p.IsDelete && p.SchoolBotLevelProposalId == id, ct);
        return row is null ? null : From(row);
    }

    private static SchoolBotLevelProposalRecord From(SchoolBotLevelProposal p) => new(p.SchoolBotLevelProposalId, p.SchoolId,
        p.CourseRunId, p.TargetCurriculumId, p.SnapshotHash, p.Snapshot, p.State, p.DecisionBy, Offset(p.DecisionAt),
        p.Reason, p.SuccessorRunId, p.CreateBy!.Value, Offset(p.CreateDate)!.Value, Offset(p.UpdateDate)!.Value);

    public Task<SchoolBotLevelProposalRecord> WriteLevelProposalAsync(SchoolBotLevelProposalRecord proposal,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        unitOfWork.RequireIdempotentCommit();
        return unitOfWork.WriteAsync(async () =>
        {
            var row = proposal.Id == 0 ? new SchoolBotLevelProposal
            {
                SchoolId = proposal.SchoolId, CourseRunId = proposal.CourseRunId, TargetCurriculumId = proposal.TargetCurriculumId,
                SnapshotHash = proposal.SnapshotHash, Snapshot = proposal.Snapshot, CreateBy = proposal.EmployeeId, CreateDate = proposal.CreatedAt.UtcDateTime
            } : await db.SchoolBotLevelProposals.SingleAsync(p => p.SchoolBotLevelProposalId == proposal.Id && p.SchoolId == proposal.SchoolId && !p.IsDelete, ct);
            row.State = proposal.State;
            row.SuccessorRunId = proposal.SuccessorRunId;
            row.DecisionBy = proposal.DecisionBy;
            row.DecisionAt = proposal.DecisionAt?.UtcDateTime;
            row.Reason = proposal.Reason;
            row.UpdateBy = audit.CreateBy;
            row.UpdateDate = proposal.UpdateDate.UtcDateTime;
            if (proposal.Id == 0) db.SchoolBotLevelProposals.Add(row);
            await db.SaveChangesAsync(ct);
            await unitOfWork.AddAuditAsync(audit, row.SchoolBotLevelProposalId, ct);
            return From(row);
        }, ct);
    }
    public async Task<SchoolBotGradingJobRecord?> GetGradingJobAsync(int submissionId, CancellationToken ct = default)
    {
        var j = await db.SchoolBotGradingJobs.AsNoTracking().SingleOrDefaultAsync(j => !j.IsDelete && j.SubmissionId == submissionId, ct);
        return j is null ? null : new(j.SchoolBotGradingJobId, j.SchoolId, j.SubmissionId, j.State, j.Attempts,
            j.LeaseOwner, Offset(j.LeaseUntil), Offset(j.NextAttemptAt), j.LastErrorCode);
    }
    public async Task<IReadOnlyList<SchoolBotAttemptRecord>> ListExpirableAttemptsAsync(int limit, CancellationToken ct = default)
    {
        var now = (await GetAssessmentTimeAsync(ct)).UtcDateTime;
        return (await db.SchoolBotAttempts.AsNoTracking().Where(a => !a.IsDelete &&
            (a.State == "started" && a.DeadlineAt != null && a.DeadlineAt <= now || a.State == "expired" && a.EarnedScore == null))
            .OrderBy(a => a.SchoolBotAttemptId).Take(Math.Clamp(limit, 1, 100)).ToArrayAsync(ct)).Select(From).ToArray();
    }
    public async Task<SchoolBotQuestionBankRecord?> GetQuestionBankByIdAsync(
        int id, CancellationToken ct = default)
    {
        Positive(id);
        var row = await db.SchoolBotQuestionBanks.AsNoTracking()
            .Where(r => r.SchoolBotQuestionBankId == id && !r.IsDelete)
            .SingleOrDefaultAsync(ct);
        return row is null ? null : From(row);
    }

    public async Task<IReadOnlyList<SchoolBotQuestionBankRecord>> ListQuestionBanksAsync(
        Guid? bankKey = null, string? lifecycle = null, CancellationToken ct = default)
    {
        if (bankKey == Guid.Empty || lifecycle is not (null or "draft" or "published" or "retired"))
            QuestionBankValidation.Invalid();
        var rows = await db.SchoolBotQuestionBanks.AsNoTracking()
            .Where(r => !r.IsDelete && (bankKey == null || r.BankKey == bankKey) && (lifecycle == null || r.Lifecycle == lifecycle))
            .OrderBy(r => r.Revision)
            .ToArrayAsync(ct);
        return rows.Select(From).ToArray();
    }

    public async Task<SchoolBotCourseRunRecord?> GetCourseRunByIdAsync(
        int id, CancellationToken ct = default)
    {
        Positive(id);
        var row = await db.SchoolBotCourseRuns.AsNoTracking()
            .Where(r => r.SchoolBotCourseRunId == id && !r.IsDelete)
            .SingleOrDefaultAsync(ct);
        return row is null ? null : From(row);
    }

    public async Task<SchoolBotCourseRunActivityRecord?> GetCourseRunActivityAsync(
        int courseRunId, int activityId, CancellationToken ct = default)
    {
        Positive(courseRunId);
        Positive(activityId);
        var row = await db.SchoolBotCourseRunActivities.AsNoTracking()
            .Where(r => r.CourseRunId == courseRunId && r.ActivityId == activityId && !r.IsDelete)
            .SingleOrDefaultAsync(ct);
        return row is null ? null : From(row);
    }

    public async Task<SchoolBotAttemptRecord?> GetAttemptByIdAsync(
        int id, CancellationToken ct = default)
    {
        Positive(id);
        var row = await db.SchoolBotAttempts.AsNoTracking()
            .Where(r => r.SchoolBotAttemptId == id && !r.IsDelete)
            .SingleOrDefaultAsync(ct);
        return row is null ? null : From(row);
    }

    public async Task<IReadOnlyList<SchoolBotAttemptRecord>> ListAttemptsAsync(
        int courseRunId, int? activityId, int learnerId, CancellationToken ct = default)
    {
        Positive(courseRunId);
        if (activityId is { } id) Positive(id);
        var rows = await db.SchoolBotAttempts.AsNoTracking()
            .Where(r => r.CourseRunId == courseRunId && r.LearnerId == learnerId && (activityId == null || r.ActivityId == activityId) && !r.IsDelete)
            .OrderBy(r => r.StartedAt)
            .ToArrayAsync(ct);
        return rows.Select(From).ToArray();
    }

    public Task<SchoolBotQuestionBankRecord> CreateQuestionBankAsync(
        SchoolBotQuestionBankRecord record, SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        unitOfWork.RequireIdempotentCommit();
        QuestionBankService.ValidateStoredNewBank(record);
        QuestionBankService.ValidateStoredBankAudit(audit, "QuestionBank.Created");
        return unitOfWork.WriteAsync(async () =>
        {
            if (await db.SchoolBotQuestionBanks.AnyAsync(r => r.BankKey == record.BankKey && r.Revision == record.Revision && !r.IsDelete, ct))
                throw new DomainRuleException(ErrorCodes.StateConflict, "Question bank revision already exists.");
            var now = unitOfWork.Now();
            var row = To(record with
            {
                SchoolBotQuestionBankId = 0, CreateBy = audit.CreateBy, UpdateBy = audit.CreateBy,
                CreateDate = now, UpdateDate = now
            });
            db.SchoolBotQuestionBanks.Add(row);
            await db.SaveChangesAsync(ct);
            await unitOfWork.AddAuditAsync(audit, row.SchoolBotQuestionBankId, ct);
            return From(row);
        }, ct);
    }

    public Task<SchoolBotQuestionBankRecord> UpdateQuestionBankDraftAsync(
        int id, string title, IReadOnlyList<QuestionBankQuestion> questions, string sourceRef,
        string expectedETag, SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        QuestionBankValidation.Validate(title, questions, sourceRef);
        QuestionBankService.ValidateStoredBankAudit(audit, "QuestionBank.Updated");
        return ChangeBankAsync(id, expectedETag, audit, (record, now) => record with
        {
            Title = title, Questions = questions, SourceRef = sourceRef, UpdateBy = audit.CreateBy, UpdateDate = now
        }, ct);
    }

    public Task<SchoolBotQuestionBankRecord> PublishQuestionBankAsync(
        int id, string expectedETag, SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        QuestionBankService.ValidateStoredBankAudit(audit, "QuestionBank.Published");
        return ChangeBankAsync(id, expectedETag, audit,
            (record, now) => QuestionBankService.PrepareStoredBankPublication(record, audit, now), ct);
    }

    private Task<SchoolBotQuestionBankRecord> ChangeBankAsync(int id, string expectedETag, SchoolBotAuditEventRecord audit,
        Func<SchoolBotQuestionBankRecord, DateTimeOffset, SchoolBotQuestionBankRecord> change, CancellationToken ct)
    {
        unitOfWork.RequireIdempotentCommit();
        return unitOfWork.WriteAsync(async () =>
        {
            var row = await db.SchoolBotQuestionBanks.SingleOrDefaultAsync(r => r.SchoolBotQuestionBankId == id && !r.IsDelete, ct)
                ?? throw SchoolBotUnitOfWork.Missing("Question bank");
            SchoolBotUnitOfWork.Match(expectedETag, row.UpdateDate, required: true);
            var existing = From(row);
            var audits = await ListAuditEventsByEntityAsync("SchoolBotQuestionBank", id, ct);
            QuestionBankService.ValidateStoredBankChange(existing, expectedETag, audit, audits, id);
            db.Entry(row).CurrentValues.SetValues(To(change(existing, unitOfWork.Now(existing.UpdateDate))));
            await unitOfWork.AddAuditAsync(audit, id, ct);
            return From(row);
        }, ct);
    }
    public Task<IReadOnlyList<SchoolBotAuditEventRecord>> ListAuditEventsByEntityAsync(string entityType, int entityId, CancellationToken ct = default) =>
        unitOfWork.Curriculum.ListAuditEventsByEntityAsync(entityType, entityId, ct);

    public Task<IdempotentCommitResult<T>> CommitWithIdempotencyAsync<T>(IdempotencyCommand command,
        Func<ISchoolBotRepositoryScope, int?, DateTimeOffset, CancellationToken, Task> recheckAccess,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<MutationResult<T>>> mutate, CancellationToken ct = default) =>
        unitOfWork.CommitWithIdempotencyAsync(command, recheckAccess, mutate, ct);

    private static void Positive(int id)
    {
        if (id <= 0) QuestionBankValidation.Invalid();
    }

    public async Task<DateTimeOffset> GetAssessmentTimeAsync(CancellationToken ct = default)
    {
        var utc = await db.Database.SqlQuery<DateTime>($"SELECT clock_timestamp() AS \"Value\"").SingleAsync(ct);
        return new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc));
    }
    public Task<SchoolBotAttemptRecord> WriteAttemptAsync(SchoolBotAttemptRecord record,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        unitOfWork.RequireIdempotentCommit();
        return unitOfWork.WriteAsync(async () =>
        {
            if (record.SchoolBotAttemptId == 0)
            {
                var created = To(record);
                db.SchoolBotAttempts.Add(created);
                await db.SaveChangesAsync(ct);
                await unitOfWork.AddAuditAsync(audit, created.SchoolBotAttemptId, ct);
                return From(created);
            }
            var row = await db.SchoolBotAttempts.SingleAsync(r =>
                r.SchoolBotAttemptId == record.SchoolBotAttemptId && r.SchoolId == record.SchoolId && !r.IsDelete, ct);
            row.State = record.State;
            row.Answers = record.Answers;
            row.SubmittedAt = record.SubmittedAt?.UtcDateTime;
            row.EarnedScore = record.EarnedScore;
            row.UpdateDate = record.UpdateDate?.UtcDateTime;
            row.UpdateBy = record.UpdateBy;
            await db.SaveChangesAsync(ct);
            await unitOfWork.AddAuditAsync(audit, row.SchoolBotAttemptId, ct);
            return From(row);
        }, ct);
    }

    private static SchoolBotQuestionBankRecord From(SchoolBotQuestionBank row) => new(
        row.SchoolBotQuestionBankId,
        row.BankKey,
        row.Revision,
        row.Title,
        JsonSerializer.Deserialize<IReadOnlyList<QuestionBankQuestion>>(row.Questions) ?? throw new InvalidOperationException("Invalid stored Questions."),
        row.SourceRef,
        row.Lifecycle,
        Offset(row.PublishedAt),
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotQuestionBank To(SchoolBotQuestionBankRecord row) => new()
    {
        SchoolBotQuestionBankId = row.SchoolBotQuestionBankId,
        BankKey = row.BankKey,
        Revision = row.Revision,
        Title = row.Title,
        Questions = JsonSerializer.Serialize(row.Questions),
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

    private static SchoolBotCourseRunRecord From(SchoolBotCourseRun row) => new(
        row.SchoolBotCourseRunId,
        row.CurriculumId,
        row.ClassroomId,
        row.Nyear,
        row.Nterm,
        row.PlanCourseIds.ToArray(),
        row.CurrentPolicyId,
        row.PreviousRunId,
        Offset(row.EffectiveAt),
        row.Lifecycle,
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotCourseRun To(SchoolBotCourseRunRecord row) => new()
    {
        SchoolBotCourseRunId = row.SchoolBotCourseRunId,
        CurriculumId = row.CurriculumId,
        ClassroomId = row.ClassroomId,
        Nyear = row.NYear,
        Nterm = row.NTerm,
        PlanCourseIds = row.PlanCourseIds.ToList(),
        CurrentPolicyId = row.CurrentPolicyId,
        PreviousRunId = row.PreviousRunId,
        EffectiveAt = row.EffectiveAt.UtcDateTime,
        Lifecycle = row.Lifecycle,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static SchoolBotCourseRunActivityRecord From(SchoolBotCourseRunActivity row) => new(
        row.SchoolBotCourseRunActivityId,
        row.CourseRunId,
        row.ActivityId,
        Offset(row.OpensAt),
        Offset(row.ClosesAt),
        row.PolicyOverride,
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotCourseRunActivity To(SchoolBotCourseRunActivityRecord row) => new()
    {
        SchoolBotCourseRunActivityId = row.SchoolBotCourseRunActivityId,
        CourseRunId = row.CourseRunId,
        ActivityId = row.ActivityId,
        OpensAt = row.OpensAt?.UtcDateTime,
        ClosesAt = row.ClosesAt?.UtcDateTime,
        PolicyOverride = row.PolicyOverride,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static SchoolBotAttemptRecord From(SchoolBotAttempt row) => new(
        row.SchoolBotAttemptId,
        row.CourseRunId,
        row.ActivityId,
        row.LearnerId,
        row.Sequence,
        row.State,
        Offset(row.StartedAt),
        Offset(row.DeadlineAt),
        Offset(row.SubmittedAt),
        row.Snapshot,
        row.Answers,
        row.EarnedScore,
        row.MaxScore,
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotAttempt To(SchoolBotAttemptRecord row) => new()
    {
        SchoolBotAttemptId = row.SchoolBotAttemptId,
        CourseRunId = row.CourseRunId,
        ActivityId = row.ActivityId,
        LearnerId = row.LearnerId,
        Sequence = row.Sequence,
        State = row.State,
        StartedAt = row.StartedAt.UtcDateTime,
        DeadlineAt = row.DeadlineAt?.UtcDateTime,
        SubmittedAt = row.SubmittedAt?.UtcDateTime,
        Snapshot = row.Snapshot,
        Answers = row.Answers,
        EarnedScore = row.EarnedScore,
        MaxScore = row.MaxScore,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static DateTimeOffset Offset(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    private static DateTimeOffset? Offset(DateTime? value) => value is { } date ? Offset(date) : null;

    public async Task<IReadOnlyList<SchoolBotGradingJobRecord>> ListOpenGradingJobsAsync(int limit, CancellationToken ct = default) =>
        (await db.SchoolBotGradingJobs.AsNoTracking().Where(j => !j.IsDelete && j.State == "queued" && j.NextAttemptAt <= DateTime.UtcNow)
            .OrderBy(j => j.SchoolBotGradingJobId).Take(Math.Clamp(limit, 1, 100)).ToArrayAsync(ct))
        .Select(j => new SchoolBotGradingJobRecord(j.SchoolBotGradingJobId, j.SchoolId, j.SubmissionId, j.State,
            j.Attempts, j.LeaseOwner, Offset(j.LeaseUntil), Offset(j.NextAttemptAt), j.LastErrorCode)).ToArray();
    public async Task<(SchoolBotProjectRecord Project, SchoolBotProjectRevisionRecord Revision)?> GetLearningProjectAsync(
        int revisionId, CancellationToken ct = default)
    {
        var r = await db.SchoolBotProjectRevisions.AsNoTracking()
            .SingleOrDefaultAsync(r => r.SchoolBotProjectRevisionId == revisionId && !r.IsDelete, ct);
        if (r is null) return null;
        var p = await unitOfWork.Project.GetProjectByIdAsync(r.ProjectId, ct);
        return p is null ? null : (p, new(r.SchoolBotProjectRevisionId, r.ProjectId, r.Revision, r.Content,
            r.ContentHash, r.IsDelete, r.CreateBy, Offset(r.CreateDate), r.UpdateBy, Offset(r.UpdateDate), r.SchoolId));
    }

    public async Task<IReadOnlyList<SchoolBotCourseRunRecord>> ListCourseRunsAsync(int schoolId, int curriculumId,
        CancellationToken ct = default) => (await db.SchoolBotCourseRuns.AsNoTracking()
        .Where(r => r.SchoolId == schoolId && (curriculumId == 0 || r.CurriculumId == curriculumId) && !r.IsDelete).ToArrayAsync(ct)).Select(From).ToArray();

    public async Task<SchoolBotSubmissionRecord?> GetSubmissionAsync(int id, CancellationToken ct = default)
    {
        var row = await db.SchoolBotSubmissions.AsNoTracking().SingleOrDefaultAsync(s => s.SchoolBotSubmissionId == id && !s.IsDelete, ct);
        return row is null ? null : From(row);
    }

    public async Task<IReadOnlyList<SchoolBotSubmissionRecord>> ListSubmissionsAsync(int schoolId, int? learnerId,
        int? missionId, CancellationToken ct = default) => (await db.SchoolBotSubmissions.AsNoTracking()
        .Where(s => !s.IsDelete && s.SchoolId == schoolId && (learnerId == null || s.LearnerId == learnerId) &&
            (missionId == null || s.MissionId == missionId)).OrderByDescending(s => s.SubmittedAt)
        .ThenByDescending(s => s.SchoolBotSubmissionId).ToArrayAsync(ct)).Select(From).ToArray();

    public async Task<IReadOnlyList<SchoolBotAssessmentRecord>> ListSubmissionAssessmentsAsync(int submissionId,
        CancellationToken ct = default) => (await db.SchoolBotAssessments.AsNoTracking()
        .Where(a => !a.IsDelete && a.SubmissionId == submissionId).OrderBy(a => a.Sequence).ToArrayAsync(ct))
        .Select(a => new SchoolBotAssessmentRecord(a.SchoolBotAssessmentId, a.SchoolId, a.SubmissionId, a.Sequence,
            a.Method, a.Outcome, a.EarnedScore, a.MaxScore, a.RuleResults, a.Feedback, a.GraderVersion,
            a.AssessorId, a.SupersedesId, Offset(a.CreateDate)!.Value)).ToArray();

    private static SchoolBotSubmissionRecord From(SchoolBotSubmission s) => new(s.SchoolBotSubmissionId,
        s.SchoolId, s.LearnerId, s.MissionId, s.CriteriaId, s.ProjectRevisionId, Offset(s.SubmittedAt),
        s.State, s.CurrentAssessmentId, s.Evidence, Offset(s.UpdateDate)!.Value);

    public Task<SchoolBotSubmissionRecord> WriteSubmissionAsync(SchoolBotSubmissionRecord submission,
        SchoolBotAssessmentRecord? assessment, SchoolBotGradingJobRecord? job, SchoolBotAuditEventRecord audit,
        CancellationToken ct = default)
    {
        unitOfWork.RequireIdempotentCommit();
        return unitOfWork.WriteAsync(async () =>
        {
            var row = submission.Id == 0 ? new SchoolBotSubmission
            {
                SchoolId = submission.SchoolId, LearnerId = submission.LearnerId, MissionId = submission.MissionId,
                CriteriaId = submission.CriteriaId, ProjectRevisionId = submission.ProjectRevisionId,
                SubmittedAt = submission.SubmittedAt.UtcDateTime, Evidence = submission.Evidence,
                CreateBy = audit.CreateBy, CreateDate = submission.SubmittedAt.UtcDateTime
            } : await db.SchoolBotSubmissions.SingleAsync(s => s.SchoolBotSubmissionId == submission.Id &&
                s.SchoolId == submission.SchoolId && !s.IsDelete, ct);
            row.State = submission.State;
            row.UpdateDate = submission.UpdateDate.UtcDateTime;
            row.UpdateBy = audit.CreateBy;
            if (submission.Id == 0) db.SchoolBotSubmissions.Add(row);
            await db.SaveChangesAsync(ct);
            if (assessment is { } a)
            {
                var result = new SchoolBotAssessment
                {
                    SchoolId = a.SchoolId, SubmissionId = row.SchoolBotSubmissionId, Sequence = a.Sequence,
                    Method = a.Method, Outcome = a.Outcome, EarnedScore = a.EarnedScore, MaxScore = a.MaxScore,
                    RuleResults = a.RuleResults, Feedback = a.Feedback, GraderVersion = a.GraderVersion,
                    AssessorId = a.AssessorId, SupersedesId = a.SupersedesId,
                    CreateBy = a.AssessorId, UpdateBy = a.AssessorId,
                    CreateDate = a.CreatedAt.UtcDateTime, UpdateDate = a.CreatedAt.UtcDateTime
                };
                db.SchoolBotAssessments.Add(result);
                await db.SaveChangesAsync(ct);
                row.CurrentAssessmentId = result.SchoolBotAssessmentId;
            }
            if (job is { } j)
            {
                var target = j.Id == 0 ? new SchoolBotGradingJob { SchoolId = j.SchoolId, SubmissionId = row.SchoolBotSubmissionId }
                    : await db.SchoolBotGradingJobs.SingleAsync(g => g.SchoolBotGradingJobId == j.Id && g.SchoolId == j.SchoolId, ct);
                target.State = j.State; target.Attempts = j.Attempts; target.LeaseOwner = j.LeaseOwner;
                target.LeaseUntil = j.LeaseUntil?.UtcDateTime; target.NextAttemptAt = j.NextAttemptAt.UtcDateTime;
                target.LastErrorCode = j.LastErrorCode;
                if (j.Id == 0) db.SchoolBotGradingJobs.Add(target);
            }
            await db.SaveChangesAsync(ct);
            await unitOfWork.AddAuditAsync(audit, row.SchoolBotSubmissionId, ct);
            return From(row);
        }, ct);
    }

    public Task<SchoolBotCourseRunActivityRecord> WriteActivityPolicyAsync(SchoolBotCourseRunActivityRecord policy,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        unitOfWork.RequireIdempotentCommit();
        return unitOfWork.WriteAsync(async () =>
        {
            var row = await db.SchoolBotCourseRunActivities.SingleAsync(r => r.SchoolBotCourseRunActivityId ==
                policy.SchoolBotCourseRunActivityId && r.SchoolId == policy.SchoolId && !r.IsDelete, ct);
            row.OpensAt = policy.OpensAt?.UtcDateTime;
            row.ClosesAt = policy.ClosesAt?.UtcDateTime;
            row.PolicyOverride = policy.PolicyOverride;
            row.UpdateBy = policy.UpdateBy;
            row.UpdateDate = policy.UpdateDate?.UtcDateTime;
            await db.SaveChangesAsync(ct);
            await unitOfWork.AddAuditAsync(audit, row.SchoolBotCourseRunActivityId, ct);
            return From(row);
        }, ct);
    }

    public async Task<IReadOnlyList<SchoolBotRetakeGrantRecord>> ListRetakeGrantsAsync(int courseRunId,
        int activityId, int learnerId, CancellationToken ct = default) => (await db.SchoolBotRetakeGrants.AsNoTracking()
        .Where(g => !g.IsDelete && g.CourseRunId == courseRunId && g.ActivityId == activityId && g.LearnerId == learnerId)
        .OrderBy(g => g.SchoolBotRetakeGrantId).ToArrayAsync(ct)).Select(g => new SchoolBotRetakeGrantRecord(
            g.SchoolBotRetakeGrantId, g.SchoolId, g.CourseRunId, g.ActivityId, g.LearnerId, g.Reason,
            g.ConsumedAttemptId, g.CreateBy!.Value, Offset(g.CreateDate)!.Value)).ToArray();

    public Task<SchoolBotRetakeGrantRecord> WriteRetakeGrantAsync(SchoolBotRetakeGrantRecord grant,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        unitOfWork.RequireIdempotentCommit();
        return unitOfWork.WriteAsync(async () =>
        {
            var row = grant.Id == 0 ? new SchoolBotRetakeGrant
            {
                SchoolId = grant.SchoolId, CourseRunId = grant.CourseRunId, ActivityId = grant.ActivityId,
                LearnerId = grant.LearnerId, Reason = grant.Reason, CreateBy = grant.EmployeeId,
                CreateDate = grant.CreatedAt.UtcDateTime
            } : await db.SchoolBotRetakeGrants.SingleAsync(g => g.SchoolBotRetakeGrantId == grant.Id &&
                g.SchoolId == grant.SchoolId && !g.IsDelete, ct);
            row.ConsumedAttemptId = grant.ConsumedAttemptId;
            row.UpdateDate = unitOfWork.Now().UtcDateTime;
            row.UpdateBy = audit.CreateBy;
            if (grant.Id == 0) db.SchoolBotRetakeGrants.Add(row);
            await db.SaveChangesAsync(ct);
            await unitOfWork.AddAuditAsync(audit, row.SchoolBotRetakeGrantId, ct);
            return grant with { Id = row.SchoolBotRetakeGrantId };
        }, ct);
    }

    public async Task<SchoolBotScoringPolicyRecord?> GetScoringPolicyAsync(int id, CancellationToken ct = default)
    {
        var row = await db.SchoolBotScoringPolicies.AsNoTracking().SingleOrDefaultAsync(p => p.SchoolBotScoringPolicyId == id && !p.IsDelete, ct);
        return row is null ? null : new(row.SchoolBotScoringPolicyId, row.SchoolId, row.CourseRunId,
            row.Revision, JsonSerializer.Deserialize<ScoringPolicy>(row.Policy)!, row.CreateBy!.Value, Offset(row.CreateDate)!.Value);
    }

    public Task<SchoolBotScoringPolicyRecord> WriteScoringPolicyAsync(SchoolBotScoringPolicyRecord policy,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        unitOfWork.RequireIdempotentCommit();
        return unitOfWork.WriteAsync(async () =>
        {
            var row = new SchoolBotScoringPolicy
            {
                SchoolId = policy.SchoolId, CourseRunId = policy.CourseRunId, Revision = policy.Revision,
                Policy = JsonSerializer.Serialize(policy.Policy), CreateBy = policy.EmployeeId, UpdateBy = policy.EmployeeId,
                CreateDate = policy.CreatedAt.UtcDateTime, UpdateDate = policy.CreatedAt.UtcDateTime
            };
            db.SchoolBotScoringPolicies.Add(row);
            await db.SaveChangesAsync(ct);
            await unitOfWork.AddAuditAsync(audit, row.SchoolBotScoringPolicyId, ct);
            return policy with { Id = row.SchoolBotScoringPolicyId };
        }, ct);
    }

    public Task<SchoolBotCourseRunRecord> WriteCourseRunAsync(SchoolBotCourseRunRecord run,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        unitOfWork.RequireIdempotentCommit();
        return unitOfWork.WriteAsync(async () =>
        {
            var row = run.SchoolBotCourseRunId == 0 ? To(run) :
                await db.SchoolBotCourseRuns.SingleAsync(r => r.SchoolBotCourseRunId == run.SchoolBotCourseRunId && r.SchoolId == run.SchoolId && !r.IsDelete, ct);
            row.CurrentPolicyId = run.CurrentPolicyId;
            row.Lifecycle = run.Lifecycle;
            row.UpdateBy = run.UpdateBy;
            row.UpdateDate = run.UpdateDate?.UtcDateTime;
            if (run.SchoolBotCourseRunId == 0) db.SchoolBotCourseRuns.Add(row);
            await db.SaveChangesAsync(ct);
            await unitOfWork.AddAuditAsync(audit, row.SchoolBotCourseRunId, ct);
            return From(row);
        }, ct);
    }

}


