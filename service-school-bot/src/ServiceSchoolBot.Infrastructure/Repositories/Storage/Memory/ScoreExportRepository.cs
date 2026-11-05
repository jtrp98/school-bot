using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed class ScoreExportRepository(SchoolBotUnitOfWork unit) : IScoreExportRepository
{
    private Task<CurriculumStoreSnapshot> Read(CancellationToken ct) => unit.State.CurriculumStore.LoadCurriculumUnitAsync(ct);
    public async Task<IReadOnlyList<GradeSlotRecord>> ListGradeSlotsAsync(int schoolId, int runId, CancellationToken ct = default) =>
        ((await Read(ct)).GradeSlots ?? []).Where(s => s.SchoolId == schoolId && s.CourseRunId == runId).ToArray();
    public async Task<IReadOnlyList<ScoreMappingRecord>> ListScoreMappingsAsync(int schoolId, int runId, CancellationToken ct = default) =>
        ((await Read(ct)).ScoreMappings ?? []).Where(s => s.SchoolId == schoolId && s.CourseRunId == runId).ToArray();
    public async Task<ScorePreviewRecord?> GetScorePreviewAsync(int id, CancellationToken ct = default) =>
        ((await Read(ct)).ScorePreviews ?? []).SingleOrDefault(s => s.Id == id);
    public async Task<ScoreExportRecord?> GetScoreExportAsync(int id, CancellationToken ct = default) =>
        ((await Read(ct)).ScoreExports ?? []).SingleOrDefault(s => s.Id == id);
    public async Task<IReadOnlyList<ScoreOutboxRecord>> ListScoreOutboxAsync(int? exportId, int limit = 50, CancellationToken ct = default) =>
        ((await Read(ct)).ScoreOutbox ?? []).Where(s => exportId is null ? s.State is "queued" or "processing" or "unknown" : s.ExportId == exportId)
            .OrderBy(s => s.Id).Take(Math.Clamp(limit, 1, 1000)).ToArray();
    public async Task<IReadOnlyList<ScoreOutboxRecord>> ListUnresolvedScoreOutboxAsync(int schoolId, IReadOnlyList<int> learnerIds, CancellationToken ct = default) =>
        ((await Read(ct)).ScoreOutbox ?? []).Where(s => s.SchoolId == schoolId && learnerIds.Contains(s.LearnerId) &&
            s.State is "queued" or "processing" or "unknown" or "conflict").ToArray();

    public Task<GradeSlotRecord> WriteGradeSlotAsync(GradeSlotRecord slot, SchoolBotAuditEventRecord audit, CancellationToken ct = default) =>
        Mutate(audit, snapshot =>
        {
            var rows = snapshot.GradeSlots ?? [];
            if (rows.Any(s => s.Id != slot.Id && s.SchoolId == slot.SchoolId && s.NGradeId == slot.NGradeId &&
                s.AssessmentId == slot.AssessmentId && s.ScoreIdentifier == slot.ScoreIdentifier && s.RetiredAt is null))
                throw new DomainRuleException("SLOT_ALREADY_OWNED", "The destination is already owned.");
            var saved = slot.Id == 0 ? slot with { Id = rows.Select(s => s.Id).DefaultIfEmpty().Max() + 1 } : slot;
            return (snapshot with { GradeSlots = [.. rows.Where(s => s.Id != saved.Id), saved] }, saved, saved.Id);
        }, ct);
    public Task<ScoreMappingRecord> WriteScoreMappingAsync(ScoreMappingRecord mapping, SchoolBotAuditEventRecord audit, CancellationToken ct = default) =>
        Mutate(audit, snapshot =>
        {
            var rows = snapshot.ScoreMappings ?? [];
            var saved = mapping.Id == 0 ? mapping with { Id = rows.Select(s => s.Id).DefaultIfEmpty().Max() + 1 } : mapping;
            return (snapshot with { ScoreMappings = [.. rows.Where(s => s.Id != saved.Id), saved] }, saved, saved.Id);
        }, ct);
    public Task<ScorePreviewRecord> WriteScorePreviewAsync(ScorePreviewRecord preview, SchoolBotAuditEventRecord audit, CancellationToken ct = default) =>
        Mutate(audit, snapshot =>
        {
            var rows = snapshot.ScorePreviews ?? [];
            var saved = preview.Id == 0 ? preview with { Id = rows.Select(s => s.Id).DefaultIfEmpty().Max() + 1 } : preview;
            return (snapshot with { ScorePreviews = [.. rows.Where(s => s.Id != saved.Id), saved] }, saved, saved.Id);
        }, ct);
    public Task<ScoreExportRecord> CreateScoreExportAsync(ScoreExportRecord export, IReadOnlyList<ScoreOutboxRecord> items,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default) => Mutate(audit, snapshot =>
        {
            var exports = snapshot.ScoreExports ?? [];
            if (exports.Any(e => e.PreviewId == export.PreviewId))
                throw new DomainRuleException("PREVIEW_ALREADY_CONFIRMED", "A preview can only produce one export.");
            var preview = (snapshot.ScorePreviews ?? []).Single(p => p.Id == export.PreviewId && p.SchoolId == export.SchoolId);
            var saved = export with { Id = exports.Select(e => e.Id).DefaultIfEmpty().Max() + 1,
                State = ScoreExportState.Resolve(items, preview.Learners.Any(l => l.SkippedCells?.Count > 0)) };
            var rows = snapshot.ScoreOutbox ?? [];
            var next = rows.Select(s => s.Id).DefaultIfEmpty().Max();
            var created = items.Select(i => i with { Id = ++next, ExportId = saved.Id }).ToArray();
            return (snapshot with { ScoreExports = [.. exports, saved], ScoreOutbox = [.. rows, .. created] }, saved, saved.Id);
        }, ct);
    public Task<ScoreOutboxRecord> WriteScoreOutboxAsync(ScoreOutboxRecord item, SchoolBotAuditEventRecord audit, CancellationToken ct = default) =>
        Mutate(audit, snapshot =>
        {
            var rows = snapshot.ScoreOutbox ?? [];
            if (!rows.Any(r => r.Id == item.Id)) throw new DomainRuleException(ErrorCodes.NotFound, "Outbox item not found.");
            if (!rows.Any(r => r.Id == item.Id && r.SchoolId == item.SchoolId && r.ExportId == item.ExportId))
                throw new DomainRuleException(ErrorCodes.SchoolMismatch, "Outbox ownership cannot change.");
            var updated = rows.Select(r => r.Id == item.Id ? item : r).ToArray();
            var parent = (snapshot.ScoreExports ?? []).Single(e => e.Id == item.ExportId && e.SchoolId == item.SchoolId);
            var preview = (snapshot.ScorePreviews ?? []).Single(p => p.Id == parent.PreviewId && p.SchoolId == parent.SchoolId);
            var state = ScoreExportState.Resolve(updated.Where(r => r.ExportId == item.ExportId).ToArray(),
                preview.Learners.Any(l => l.SkippedCells?.Count > 0));
            return (snapshot with { ScoreOutbox = updated, ScoreExports = (snapshot.ScoreExports ?? [])
                .Select(e => e.Id == item.ExportId ? e with { State = state, UpdateDate = item.UpdateDate } : e).ToArray() }, item, item.Id);
        }, ct);
    private Task<T> Mutate<T>(SchoolBotAuditEventRecord audit,
        Func<CurriculumStoreSnapshot, (CurriculumStoreSnapshot Snapshot, T Result, int Id)> action, CancellationToken ct)
    {
        if (!unit.State.isCurriculumTransaction) throw new InvalidOperationException("Atomic score transaction required.");
        return unit.State.CurriculumStore.MutateCurriculumUnitAsync((snapshot, _) =>
        {
            var (updated, result, id) = action(snapshot);
            var row = audit with { EntityId = id, SchoolBotAuditEventId = snapshot.AuditEvents.Select(a => a.SchoolBotAuditEventId).DefaultIfEmpty().Max() + 1 };
            return Task.FromResult(new CurriculumStoreMutation<T>(updated with { AuditEvents = [.. snapshot.AuditEvents, row] }, result));
        }, ct);
    }
}
