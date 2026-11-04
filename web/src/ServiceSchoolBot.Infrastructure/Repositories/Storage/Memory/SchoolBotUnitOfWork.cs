using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Identity;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Domain.Projects;

using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed partial class SchoolBotUnitOfWork : ISchoolBotRepositoryScope, IIdempotentSchoolBotCommit
{
    internal SchoolBotRepositoryState State { get; }
    public ProjectRepository Project { get; }
    public WebSessionRepository WebSession { get; }
    public LicenseRepository License { get; }
    public CurriculumRepository Curriculum { get; }
    public CurriculumImportRepository CurriculumImport { get; }
    public AssessmentRepository Assessment { get; }
    public IdempotencyRepository Idempotency { get; }

    public SchoolBotUnitOfWork(ISchoolBotBackingStore backingStore, TimeProvider? clock = null)
    {
        State = new SchoolBotRepositoryState(backingStore, clock);
        Project = new ProjectRepository(this);
        WebSession = new WebSessionRepository(this);
        License = new LicenseRepository(this);
        Curriculum = new CurriculumRepository(this);
        CurriculumImport = new CurriculumImportRepository(this);
        Assessment = new AssessmentRepository(this);
        Idempotency = new IdempotencyRepository(this);
    }

    public Task<IdempotentCommitResult<T>> CommitWithIdempotencyAsync<T>(
        IdempotencyCommand command,
        Func<ISchoolBotRepositoryScope, int?, DateTimeOffset, CancellationToken, Task> recheckAccess,
        Func<ISchoolBotRepositoryScope, DateTimeOffset, CancellationToken, Task<MutationResult<T>>> mutate,
        CancellationToken ct = default) =>
        Idempotency.CommitWithIdempotencyAsync(command, recheckAccess, mutate, ct);

    internal async Task<(SchoolBotUnitOfWork Adapter, MemorySchoolBotBackingStore Store)> CreateTransactionAsync(
        CurriculumStoreSnapshot snapshot, CancellationToken ct)
    {
        var store = new MemorySchoolBotBackingStore();
        await store.MutateCurriculumUnitAsync((_, token) => Task.FromResult(
            new CurriculumStoreMutation<bool>(snapshot, true)), ct);
        return (new SchoolBotUnitOfWork(store, State.clock) { State = { isCurriculumTransaction = true } }, store);
    }

    internal Task<T> CommitCurriculumMutationAsync<T>(
        Func<SchoolBotUnitOfWork, Task<T>> mutate, CancellationToken ct) =>
        State.CurriculumStore.MutateCurriculumUnitAsync(async (snapshot, token) =>
        {
            var scope = await CreateTransactionAsync(snapshot, token);
            var transaction = scope.Adapter;
            var result = await mutate(transaction);
            var candidate = await scope.Store.LoadCurriculumUnitAsync(token);
            return new CurriculumStoreMutation<T>(candidate, result);
        }, ct);

    public Task<SchoolBotQuestionBankRecord?> GetQuestionBankByIdAsync(int id, CancellationToken ct = default) =>
        Assessment.GetQuestionBankByIdAsync(id, ct);

    public Task<IReadOnlyList<SchoolBotQuestionBankRecord>> ListQuestionBanksAsync(
        Guid? bankKey = null, string? lifecycle = null, CancellationToken ct = default) =>
        Assessment.ListQuestionBanksAsync(bankKey, lifecycle, ct);

    public Task<SchoolBotCourseRunRecord?> GetCourseRunByIdAsync(int id, CancellationToken ct = default) =>
        Assessment.GetCourseRunByIdAsync(id, ct);

    public Task<SchoolBotCourseRunActivityRecord?> GetCourseRunActivityAsync(
        int courseRunId, int activityId, CancellationToken ct = default) =>
        Assessment.GetCourseRunActivityAsync(courseRunId, activityId, ct);

    public Task<SchoolBotAttemptRecord?> GetAttemptByIdAsync(int id, CancellationToken ct = default) =>
        Assessment.GetAttemptByIdAsync(id, ct);

    public Task<IReadOnlyList<SchoolBotAttemptRecord>> ListAttemptsAsync(
        int courseRunId, int? activityId, int learnerId, CancellationToken ct = default) =>
        Assessment.ListAttemptsAsync(courseRunId, activityId, learnerId, ct);

    public Task<SchoolBotQuestionBankRecord> CreateQuestionBankAsync(
        SchoolBotQuestionBankRecord record, SchoolBotAuditEventRecord audit, CancellationToken ct = default) =>
        Assessment.CreateQuestionBankAsync(record, audit, ct);

    public Task<SchoolBotQuestionBankRecord> UpdateQuestionBankDraftAsync(
        int id, string title, IReadOnlyList<QuestionBankQuestion> questions, string sourceRef,
        string expectedETag, SchoolBotAuditEventRecord audit, CancellationToken ct = default) =>
        Assessment.UpdateQuestionBankDraftAsync(id, title, questions, sourceRef, expectedETag, audit, ct);

    public Task<SchoolBotQuestionBankRecord> PublishQuestionBankAsync(
        int id, string expectedETag, SchoolBotAuditEventRecord audit, CancellationToken ct = default) =>
        Assessment.PublishQuestionBankAsync(id, expectedETag, audit, ct);

    public Task<SchoolBotLicenseRecord?> GetLicenseByIdAsync(int licenseId, CancellationToken ct = default) =>
        License.GetLicenseByIdAsync(licenseId, ct);

    public Task<SchoolBotLicenseRecord?> GetLicenseByKeyHashAsync(string keyHash, CancellationToken ct = default) =>
        License.GetLicenseByKeyHashAsync(keyHash, ct);

    public Task<IReadOnlyList<SchoolBotLicenseRecord>> ListLicensesAsync(int? schoolId = null, CancellationToken ct = default) =>
        License.ListLicensesAsync(schoolId, ct);

    public Task<SchoolBotLicenseRecord> CreateLicenseAsync(SchoolBotLicenseRecord license, CancellationToken ct = default) =>
        License.CreateLicenseAsync(license, ct);

    public Task<SchoolBotLicenseRecord> RevokeLicenseAsync(int licenseId, int? revokedBy, string? expectedETag = null, CancellationToken ct = default) =>
        License.RevokeLicenseAsync(licenseId, revokedBy, expectedETag, ct);

    public Task<SchoolBotLicenseBindingRecord?> GetBindingByIdAsync(int bindingId, CancellationToken ct = default) =>
        License.GetBindingByIdAsync(bindingId, ct);

    public Task<IReadOnlyList<SchoolBotLicenseBindingRecord>> ListBindingsByLicenseIdAsync(int licenseId, CancellationToken ct = default) =>
        License.ListBindingsByLicenseIdAsync(licenseId, ct);

    public Task<SchoolBotLicenseBindingRecord> UpdateBindingAsync(
        SchoolBotLicenseBindingRecord binding, CancellationToken ct = default,
        Action<LicenseActivationCommitResult>? prepare = null) =>
        License.UpdateBindingAsync(binding, ct, prepare);

    public Task<LicenseActivationCommitResult> CommitLicenseActivationAsync(
        CommitLicenseActivationRequest request, CancellationToken ct = default,
        Action<LicenseActivationCommitResult>? prepare = null) =>
        License.CommitLicenseActivationAsync(request, ct, prepare);

    public Task<SchoolBotLicenseRecord> CommitLicenseUnbindAsync(CommitLicenseUnbindRequest request, CancellationToken ct = default) =>
        License.CommitLicenseUnbindAsync(request, ct);

    public Task<ProjectCommitResult> CommitProjectAsync(CommitProjectRequest request, CancellationToken ct = default) =>
        Project.CommitProjectAsync(request, ct);

    public Task<SchoolBotProjectRecord?> GetProjectByIdAsync(int projectId, CancellationToken ct = default) =>
        Project.GetProjectByIdAsync(projectId, ct);

    public Task<IReadOnlyList<SchoolBotProjectRecord>> ListProjectsByOwnerAsync(int schoolId, string ownerKind, int ownerId, CancellationToken ct = default) =>
        Project.ListProjectsByOwnerAsync(schoolId, ownerKind, ownerId, ct);

    public Task<IReadOnlyList<SchoolBotProjectRevisionRecord>> ListProjectRevisionsAsync(int projectId, CancellationToken ct = default) =>
        Project.ListProjectRevisionsAsync(projectId, ct);

    public Task<SchoolBotProjectRevisionRecord?> GetProjectRevisionAsync(int projectId, int revisionNumber, CancellationToken ct = default) =>
        Project.GetProjectRevisionAsync(projectId, revisionNumber, ct);

    public Task<SchoolBotProjectRecord> SoftDeleteProjectAsync(int projectId, string? expectedETag = null, int? deletedBy = null, CancellationToken ct = default) =>
        Project.SoftDeleteProjectAsync(projectId, expectedETag, deletedBy, ct);

    public Task<SchoolBotSessionRecord> CreateSessionAsync(SchoolBotSessionRecord session, CancellationToken ct = default) =>
        WebSession.CreateSessionAsync(session, ct);

    public Task<SchoolBotSessionRecord?> GetSessionByTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        WebSession.GetSessionByTokenHashAsync(tokenHash, ct);

    public Task<SchoolBotSessionRecord> UpdateSessionAsync(SchoolBotSessionRecord session, string? expectedETag = null, CancellationToken ct = default) =>
        WebSession.UpdateSessionAsync(session, expectedETag, ct);

    public Task RevokeSessionAsync(string tokenHash, int? revokedBy, CancellationToken ct = default) =>
        WebSession.RevokeSessionAsync(tokenHash, revokedBy, ct);

    public Task<SchoolBotCurriculumRecord?> GetCurriculumByIdAsync(int curriculumId, CancellationToken ct = default) =>
        Curriculum.GetCurriculumByIdAsync(curriculumId, ct);

    public Task<SchoolBotCurriculumRecord?> GetCurriculumByKeyAndRevisionAsync(Guid curriculumKey, int revision, CancellationToken ct = default) =>
        Curriculum.GetCurriculumByKeyAndRevisionAsync(curriculumKey, revision, ct);

    public Task<IReadOnlyList<SchoolBotCurriculumRecord>> ListCurriculaAsync(int? levelId = null, string? lifecycle = null, CancellationToken ct = default) =>
        Curriculum.ListCurriculaAsync(levelId, lifecycle, ct);

    public Task<IReadOnlyList<SchoolBotLessonRecord>> ListLessonsByCurriculumIdAsync(int curriculumId, CancellationToken ct = default) =>
        Curriculum.ListLessonsByCurriculumIdAsync(curriculumId, ct);

    public Task<IReadOnlyList<SchoolBotMissionRecord>> ListMissionsByLessonIdAsync(int lessonId, CancellationToken ct = default) =>
        Curriculum.ListMissionsByLessonIdAsync(lessonId, ct);

    public Task<IReadOnlyList<SchoolBotCriteriaRecord>> ListCriteriaByMissionIdAsync(int missionId, CancellationToken ct = default) =>
        Curriculum.ListCriteriaByMissionIdAsync(missionId, ct);

    public Task<IReadOnlyList<SchoolBotActivityRecord>> ListActivitiesByLessonIdAsync(int lessonId, CancellationToken ct = default) =>
        Curriculum.ListActivitiesByLessonIdAsync(lessonId, ct);

    public Task<IReadOnlyList<SchoolBotAuditEventRecord>> ListAuditEventsByEntityAsync(string entityType, int entityId, CancellationToken ct = default) =>
        Curriculum.ListAuditEventsByEntityAsync(entityType, entityId, ct);

    public Task<SchoolBotCurriculumRecord> CreateCurriculumAsync(
        SchoolBotCurriculumRecord curriculum,
        SchoolBotAuditEventRecord initialAudit,
        CancellationToken ct = default) =>
        Curriculum.CreateCurriculumAsync(curriculum, initialAudit, ct);

    public Task<SchoolBotCurriculumRecord> UpdateCurriculumMetadataAsync(
        int curriculumId,
        int levelId,
        string title,
        string sourceRef,
        IReadOnlyList<string> recommendedGrades,
        IReadOnlyList<CurriculumSubject> subjects,
        string expectedETag,
        SchoolBotAuditEventRecord auditEvent,
        CancellationToken ct = default) =>
        Curriculum.UpdateCurriculumMetadataAsync(curriculumId, levelId, title, sourceRef, recommendedGrades, subjects, expectedETag, auditEvent, ct);

    public Task<SchoolBotCurriculumRecord> UpdateCurriculumContentAsync(
        int curriculumId,
        IReadOnlyList<SchoolBotLessonRecord> newLessons,
        IReadOnlyList<SchoolBotMissionRecord> newMissions,
        IReadOnlyList<SchoolBotCriteriaRecord> newCriteria,
        IReadOnlyList<SchoolBotActivityRecord> newActivities,
        string expectedETag,
        SchoolBotAuditEventRecord auditEvent,
        CancellationToken ct = default) =>
        Curriculum.UpdateCurriculumContentAsync(curriculumId, newLessons, newMissions, newCriteria, newActivities, expectedETag, auditEvent, ct);

    public Task<CurriculumImportResponse> ImportCurriculumLessonsAsync(
        int curriculumId,
        IReadOnlyList<PreparedCurriculumImportRow> rows,
        SchoolBotAuditEventRecord audit,
        CancellationToken ct = default) =>
        CurriculumImport.ImportCurriculumLessonsAsync(curriculumId, rows, audit, ct);

    public Task<SchoolBotCurriculumRecord> PublishCurriculumAsync(
        int curriculumId,
        string approvalRef,
        string expectedETag,
        SchoolBotAuditEventRecord publishAudit,
        CancellationToken ct = default) =>
        Curriculum.PublishCurriculumAsync(curriculumId, approvalRef, expectedETag, publishAudit, ct);

}
