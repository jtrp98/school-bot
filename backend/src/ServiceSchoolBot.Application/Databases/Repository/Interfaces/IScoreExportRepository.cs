using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface IScoreExportRepository
{
    Task<IReadOnlyList<GradeSlotRecord>> ListGradeSlotsAsync(int schoolId, int runId, CancellationToken ct = default);
    Task<IReadOnlyList<ScoreMappingRecord>> ListScoreMappingsAsync(int schoolId, int runId, CancellationToken ct = default);
    Task<GradeSlotRecord> WriteGradeSlotAsync(GradeSlotRecord slot, SchoolBotAuditEventRecord audit, CancellationToken ct = default);
    Task<ScoreMappingRecord> WriteScoreMappingAsync(ScoreMappingRecord mapping, SchoolBotAuditEventRecord audit, CancellationToken ct = default);
    Task<ScorePreviewRecord?> GetScorePreviewAsync(int id, CancellationToken ct = default);
    Task<ScorePreviewRecord> WriteScorePreviewAsync(ScorePreviewRecord preview, SchoolBotAuditEventRecord audit, CancellationToken ct = default);
    Task<ScoreExportRecord?> GetScoreExportAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ScoreOutboxRecord>> ListScoreOutboxAsync(int? exportId, int limit = 50, CancellationToken ct = default);
    Task<IReadOnlyList<ScoreOutboxRecord>> ListUnresolvedScoreOutboxAsync(int schoolId, IReadOnlyList<int> learnerIds, CancellationToken ct = default);
    Task<ScoreExportRecord> CreateScoreExportAsync(ScoreExportRecord export, IReadOnlyList<ScoreOutboxRecord> items,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default);
    Task<ScoreOutboxRecord> WriteScoreOutboxAsync(ScoreOutboxRecord item, SchoolBotAuditEventRecord audit, CancellationToken ct = default);
}
