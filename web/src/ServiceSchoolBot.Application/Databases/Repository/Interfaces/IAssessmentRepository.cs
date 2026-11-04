using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface IAssessmentRepository
{
    Task<SchoolBotQuestionBankRecord?> GetQuestionBankByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotQuestionBankRecord>> ListQuestionBanksAsync(
        Guid? bankKey = null, string? lifecycle = null, CancellationToken ct = default);
    Task<SchoolBotCourseRunRecord?> GetCourseRunByIdAsync(int id, CancellationToken ct = default);
    Task<SchoolBotCourseRunActivityRecord?> GetCourseRunActivityAsync(
        int courseRunId, int activityId, CancellationToken ct = default);
    Task<SchoolBotAttemptRecord?> GetAttemptByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotAttemptRecord>> ListAttemptsAsync(
        int courseRunId, int? activityId, int learnerId, CancellationToken ct = default);
    Task<SchoolBotQuestionBankRecord> CreateQuestionBankAsync(
        SchoolBotQuestionBankRecord record, SchoolBotAuditEventRecord audit, CancellationToken ct = default);
    Task<SchoolBotQuestionBankRecord> UpdateQuestionBankDraftAsync(
        int id, string title, IReadOnlyList<QuestionBankQuestion> questions, string sourceRef,
        string expectedETag, SchoolBotAuditEventRecord audit, CancellationToken ct = default);
    Task<SchoolBotQuestionBankRecord> PublishQuestionBankAsync(
        int id, string expectedETag, SchoolBotAuditEventRecord audit, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotAuditEventRecord>> ListAuditEventsByEntityAsync(
        string entityType, int entityId, CancellationToken ct = default);
}
