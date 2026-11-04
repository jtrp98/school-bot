using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed partial class AssessmentRepository(SchoolBotDbContext db, SchoolBotUnitOfWork unitOfWork) : IAssessmentRepository, IIdempotentSchoolBotCommit
{
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
}
