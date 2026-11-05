using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface IAssessmentRepository
{
    Task<SchoolBotCurriculumRecord> ApproveLearningCriteriaAsync(int curriculumId, string actor, DateTimeOffset now,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotLevelRecord>> ListLevelsAsync(CancellationToken ct = default);
    Task<SchoolBotLevelPromotionPolicyRecord?> GetPromotionPolicyAsync(int levelId, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotLevelProposalRecord>> ListLevelProposalsAsync(int courseRunId, CancellationToken ct = default);
    Task<SchoolBotLevelProposalRecord?> GetLevelProposalAsync(int id, CancellationToken ct = default);
    Task<SchoolBotLevelProposalRecord> WriteLevelProposalAsync(SchoolBotLevelProposalRecord proposal, SchoolBotAuditEventRecord audit, CancellationToken ct = default);
    Task<SchoolBotGradingJobRecord?> GetGradingJobAsync(int submissionId, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotAttemptRecord>> ListExpirableAttemptsAsync(int limit, CancellationToken ct = default);
    Task<SchoolBotCourseRunActivityRecord> WriteActivityPolicyAsync(SchoolBotCourseRunActivityRecord policy, SchoolBotAuditEventRecord audit, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotRetakeGrantRecord>> ListRetakeGrantsAsync(int courseRunId, int activityId, int learnerId, CancellationToken ct = default);
    Task<SchoolBotRetakeGrantRecord> WriteRetakeGrantAsync(SchoolBotRetakeGrantRecord grant, SchoolBotAuditEventRecord audit, CancellationToken ct = default);
    Task<SchoolBotScoringPolicyRecord?> GetScoringPolicyAsync(int id, CancellationToken ct = default);
    Task<SchoolBotScoringPolicyRecord> WriteScoringPolicyAsync(SchoolBotScoringPolicyRecord policy, SchoolBotAuditEventRecord audit, CancellationToken ct = default);
    Task<SchoolBotCourseRunRecord> WriteCourseRunAsync(SchoolBotCourseRunRecord run, SchoolBotAuditEventRecord audit, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotGradingJobRecord>> ListOpenGradingJobsAsync(int limit, CancellationToken ct = default);
    Task<(ServiceSchoolBot.Domain.Projects.SchoolBotProjectRecord Project, ServiceSchoolBot.Domain.Projects.SchoolBotProjectRevisionRecord Revision)?> GetLearningProjectAsync(int revisionId, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotCourseRunRecord>> ListCourseRunsAsync(int schoolId, int curriculumId, CancellationToken ct = default);
    Task<SchoolBotSubmissionRecord?> GetSubmissionAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotSubmissionRecord>> ListSubmissionsAsync(int schoolId, int? learnerId, int? missionId, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotAssessmentRecord>> ListSubmissionAssessmentsAsync(int submissionId, CancellationToken ct = default);
    Task<SchoolBotSubmissionRecord> WriteSubmissionAsync(SchoolBotSubmissionRecord submission,
        SchoolBotAssessmentRecord? assessment, SchoolBotGradingJobRecord? job, SchoolBotAuditEventRecord audit, CancellationToken ct = default);
    Task<DateTimeOffset> GetAssessmentTimeAsync(CancellationToken ct = default);
    Task<SchoolBotAttemptRecord> WriteAttemptAsync(SchoolBotAttemptRecord record,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default);
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
