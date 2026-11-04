using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed class AssessmentRepository : IAssessmentRepository, IIdempotentSchoolBotCommit
{
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

}
