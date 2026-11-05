using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed class ScoreExportRepository(SchoolBotDbContext db, SchoolBotUnitOfWork unit) : IScoreExportRepository
{
    public async Task<IReadOnlyList<GradeSlotRecord>> ListGradeSlotsAsync(int schoolId, int runId, CancellationToken ct = default) =>
        (await db.SchoolBotGradeSlots.AsNoTracking().Where(s => s.SchoolId == schoolId && s.CourseRunId == runId).ToArrayAsync(ct))
        .Select(s => new GradeSlotRecord(s.SchoolBotGradeSlotId, s.SchoolId, s.CourseRunId, s.PlanCourseId, s.CourseId,
            s.NgradeId, s.AssessmentId, s.ScoreIdentifier, s.Category, s.TargetSnapshot, s.OwnershipReason,
            Offset(s.RetiredAt), Offset(s.UpdateDate)!.Value)).ToArray();
    public async Task<IReadOnlyList<ScoreMappingRecord>> ListScoreMappingsAsync(int schoolId, int runId, CancellationToken ct = default) =>
        (await db.SchoolBotScoreMappings.AsNoTracking().Where(s => s.SchoolId == schoolId && s.CourseRunId == runId).ToArrayAsync(ct))
        .Select(s => new ScoreMappingRecord(s.SchoolBotScoreMappingId, s.SchoolId, s.CourseRunId, s.GradeSlotId,
            JsonSerializer.Deserialize<ScoreMappingSource[]>(s.Sources)!.Single().ActivityId, Offset(s.RetiredAt), Offset(s.UpdateDate)!.Value)).ToArray();
    public async Task<ScorePreviewRecord?> GetScorePreviewAsync(int id, CancellationToken ct = default)
    {
        var row = await db.SchoolBotScorePreviews.AsNoTracking().SingleOrDefaultAsync(s => s.SchoolBotScorePreviewId == id && !s.IsDelete, ct);
        if (row is null) return null;
        var snapshot = JsonSerializer.Deserialize<ScorePreviewRecord>(row.Context)!;
        return snapshot with { Id = id, State = row.State, UpdateDate = Offset(row.UpdateDate)!.Value };
    }
    public async Task<ScoreExportRecord?> GetScoreExportAsync(int id, CancellationToken ct = default)
    {
        var row = await db.SchoolBotScoreExports.AsNoTracking().SingleOrDefaultAsync(s => s.SchoolBotScoreExportId == id && !s.IsDelete, ct);
        return row is null ? null : new(id, row.SchoolId, row.ScorePreviewId, row.ConfirmedBy, row.CreateBy!.Value,
            Offset(row.ConfirmedAt), row.State, Offset(row.UpdateDate)!.Value);
    }
    public async Task<IReadOnlyList<ScoreOutboxRecord>> ListScoreOutboxAsync(int? exportId, int limit = 50, CancellationToken ct = default)
    {
        var query = db.SchoolBotScoreOutboxes.AsNoTracking().Where(s => !s.IsDelete);
        query = exportId is { } id ? query.Where(s => s.ScoreExportId == id) : query.Where(s => s.State == "queued" || s.State == "processing" || s.State == "unknown");
        var rows = await (from o in query
            join p in db.SchoolBotScorePreviewItems.AsNoTracking()
                on new { o.SchoolId, Id = o.ScorePreviewItemId } equals new { p.SchoolId, Id = p.SchoolBotScorePreviewItemId }
            join e in db.SchoolBotScoreExports.AsNoTracking()
                on new { o.SchoolId, Id = o.ScoreExportId } equals new { e.SchoolId, Id = e.SchoolBotScoreExportId }
            where !e.IsDelete && e.ScorePreviewId == p.ScorePreviewId
            orderby o.SchoolBotScoreOutboxId
            select new { Outbox = o, p.LearnerId }).Take(Math.Clamp(limit, 1, 1000)).ToArrayAsync(ct);
        return rows.Select(r => new ScoreOutboxRecord(r.Outbox.SchoolBotScoreOutboxId, r.Outbox.SchoolId,
            r.Outbox.ScoreExportId, r.LearnerId, r.Outbox.OperationKey, r.Outbox.PayloadHash, r.Outbox.State,
            r.Outbox.Attempts, r.Outbox.LeaseOwner, Offset(r.Outbox.LeaseUntil), Offset(r.Outbox.NextAttemptAt),
            r.Outbox.ReceiptId, r.Outbox.ReceiptHash, r.Outbox.LastErrorCode, Offset(r.Outbox.UpdateDate)!.Value)).ToArray();
    }
    public async Task<IReadOnlyList<ScoreOutboxRecord>> ListUnresolvedScoreOutboxAsync(int schoolId,
        IReadOnlyList<int> learnerIds, CancellationToken ct = default)
    {
        var rows = await (from o in db.SchoolBotScoreOutboxes.AsNoTracking()
            join p in db.SchoolBotScorePreviewItems.AsNoTracking()
                on new { o.SchoolId, Id = o.ScorePreviewItemId } equals new { p.SchoolId, Id = p.SchoolBotScorePreviewItemId }
            join e in db.SchoolBotScoreExports.AsNoTracking()
                on new { o.SchoolId, Id = o.ScoreExportId } equals new { e.SchoolId, Id = e.SchoolBotScoreExportId }
            where o.SchoolId == schoolId && !o.IsDelete && !e.IsDelete && e.ScorePreviewId == p.ScorePreviewId &&
                learnerIds.Contains(p.LearnerId) && (o.State == "queued" || o.State == "processing" || o.State == "unknown" || o.State == "conflict")
            orderby o.SchoolBotScoreOutboxId
            select new { Outbox = o, p.LearnerId }).Take(10001).ToArrayAsync(ct);
        // A truncated scan cannot prove that a second export is safe.
        if (rows.Length > 10000) throw new DomainRuleException(ErrorCodes.ServiceUnavailable, "Reconcile pending grade exports before confirming another batch.");
        return rows.Select(r => new ScoreOutboxRecord(r.Outbox.SchoolBotScoreOutboxId, r.Outbox.SchoolId,
            r.Outbox.ScoreExportId, r.LearnerId, r.Outbox.OperationKey, r.Outbox.PayloadHash, r.Outbox.State,
            r.Outbox.Attempts, r.Outbox.LeaseOwner, Offset(r.Outbox.LeaseUntil), Offset(r.Outbox.NextAttemptAt),
            r.Outbox.ReceiptId, r.Outbox.ReceiptHash, r.Outbox.LastErrorCode, Offset(r.Outbox.UpdateDate)!.Value)).ToArray();
    }

    public async Task<GradeSlotRecord> WriteGradeSlotAsync(GradeSlotRecord slot, SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        unit.RequireIdempotentCommit();
        if (await db.SchoolBotGradeSlots.AnyAsync(s => s.SchoolBotGradeSlotId != slot.Id && s.SchoolId == slot.SchoolId &&
            s.NgradeId == slot.NGradeId && s.AssessmentId == slot.AssessmentId && s.ScoreIdentifier == slot.ScoreIdentifier && s.RetiredAt == null, ct))
            throw new DomainRuleException("SLOT_ALREADY_OWNED", "Destination is already owned.");
        var row = new SchoolBotGradeSlot { SchoolId = slot.SchoolId, CourseRunId = slot.CourseRunId,
            PlanCourseId = slot.PlanCourseId, CourseId = slot.CourseId, NgradeId = slot.NGradeId, AssessmentId = slot.AssessmentId,
            ScoreIdentifier = slot.ScoreIdentifier, Category = slot.Category, TargetSnapshot = slot.TargetSnapshot,
            OwnershipReason = slot.OwnershipReason, CreateBy = audit.CreateBy, CreateDate = slot.UpdateDate.UtcDateTime,
            UpdateBy = audit.UpdateBy, UpdateDate = slot.UpdateDate.UtcDateTime };
        db.SchoolBotGradeSlots.Add(row);
        await db.SaveChangesAsync(ct);
        await unit.AddAuditAsync(audit, row.SchoolBotGradeSlotId, ct);
        return slot with { Id = row.SchoolBotGradeSlotId };
    }
    public async Task<ScoreMappingRecord> WriteScoreMappingAsync(ScoreMappingRecord mapping, SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        unit.RequireIdempotentCommit();
        var row = mapping.Id == 0 ? new SchoolBotScoreMapping() : await db.SchoolBotScoreMappings.SingleAsync(s => s.SchoolBotScoreMappingId == mapping.Id, ct);
        if (mapping.Id == 0) db.SchoolBotScoreMappings.Add(row);
        row.SchoolId = mapping.SchoolId; row.CourseRunId = mapping.CourseRunId; row.GradeSlotId = mapping.GradeSlotId;
        row.Sources = JsonSerializer.Serialize(new[] { new ScoreMappingSource(mapping.ActivityId) }); row.RetiredAt = mapping.RetiredAt?.UtcDateTime;
        row.CreateBy ??= audit.CreateBy; row.CreateDate ??= mapping.UpdateDate.UtcDateTime; row.UpdateBy = audit.UpdateBy; row.UpdateDate = mapping.UpdateDate.UtcDateTime;
        await db.SaveChangesAsync(ct);
        await unit.AddAuditAsync(audit, row.SchoolBotScoreMappingId, ct);
        return mapping with { Id = row.SchoolBotScoreMappingId };
    }
    public async Task<ScorePreviewRecord> WriteScorePreviewAsync(ScorePreviewRecord preview, SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        unit.RequireIdempotentCommit();
        var row = preview.Id == 0 ? new SchoolBotScorePreview() : await db.SchoolBotScorePreviews.SingleAsync(s => s.SchoolBotScorePreviewId == preview.Id, ct);
        if (preview.Id == 0) db.SchoolBotScorePreviews.Add(row);
        row.SchoolId = preview.SchoolId; row.CourseRunId = preview.CourseRunId; row.Principal = preview.Principal;
        row.State = preview.State; row.SnapshotHash = preview.SnapshotHash; row.ExpiresAt = preview.ExpiresAt.UtcDateTime;
        row.Context = JsonSerializer.Serialize(preview); row.CreateDate ??= preview.UpdateDate.UtcDateTime;
        row.UpdateDate = preview.UpdateDate.UtcDateTime; row.CreateBy ??= audit.CreateBy; row.UpdateBy = audit.UpdateBy;
        await db.SaveChangesAsync(ct);
        if (preview.Id == 0)
            foreach (var learner in preview.Learners) db.SchoolBotScorePreviewItems.Add(new()
            {
                ScorePreviewId = row.SchoolBotScorePreviewId, LearnerId = learner.LearnerId, State = learner.State,
                ReasonCodes = JsonSerializer.Serialize(learner.Reasons), Snapshot = JsonSerializer.Serialize(learner),
                SchoolId = preview.SchoolId, CreateDate = preview.UpdateDate.UtcDateTime, UpdateDate = preview.UpdateDate.UtcDateTime
            });
        await unit.AddAuditAsync(audit, row.SchoolBotScorePreviewId, ct);
        return preview with { Id = row.SchoolBotScorePreviewId };
    }
    public async Task<ScoreExportRecord> CreateScoreExportAsync(ScoreExportRecord export, IReadOnlyList<ScoreOutboxRecord> items,
        SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        unit.RequireIdempotentCommit();
        if (await db.SchoolBotScoreExports.AnyAsync(e => e.ScorePreviewId == export.PreviewId, ct))
            throw new DomainRuleException("PREVIEW_ALREADY_CONFIRMED", "A preview can only produce one export.");
        var preview = await GetScorePreviewAsync(export.PreviewId, ct) ?? throw new DomainRuleException(ErrorCodes.NotFound, "Preview not found.");
        if (preview.SchoolId != export.SchoolId) throw new DomainRuleException(ErrorCodes.SchoolMismatch, "Preview school changed.");
        export = export with { State = ScoreExportState.Resolve(items, preview.Learners.Any(l => l.SkippedCells?.Count > 0)) };
        var row = new SchoolBotScoreExport { SchoolId = export.SchoolId, ScorePreviewId = export.PreviewId,
            ConfirmedBy = export.ConfirmedBy, ConfirmedAt = export.ConfirmedAt.UtcDateTime, State = export.State,
            CreateBy = export.EmployeeId, UpdateBy = export.EmployeeId, CreateDate = export.ConfirmedAt.UtcDateTime, UpdateDate = export.UpdateDate.UtcDateTime };
        db.SchoolBotScoreExports.Add(row); await db.SaveChangesAsync(ct);
        var previews = await db.SchoolBotScorePreviewItems.Where(p => p.ScorePreviewId == export.PreviewId && p.SchoolId == export.SchoolId).ToArrayAsync(ct);
        foreach (var item in items) db.SchoolBotScoreOutboxes.Add(new()
        {
            SchoolId = export.SchoolId, ScoreExportId = row.SchoolBotScoreExportId,
            ScorePreviewItemId = previews.Single(p => p.LearnerId == item.LearnerId).SchoolBotScorePreviewItemId,
            OperationKey = item.OperationKey, PayloadHash = item.PayloadHash, State = item.State, NextAttemptAt = item.NextAttemptAt.UtcDateTime,
            LastErrorCode = item.LastErrorCode,
            CreateDate = item.UpdateDate.UtcDateTime, UpdateDate = item.UpdateDate.UtcDateTime
        });
        await unit.AddAuditAsync(audit, row.SchoolBotScoreExportId, ct);
        return export with { Id = row.SchoolBotScoreExportId };
    }
    public async Task<ScoreOutboxRecord> WriteScoreOutboxAsync(ScoreOutboxRecord item, SchoolBotAuditEventRecord audit, CancellationToken ct = default)
    {
        unit.RequireIdempotentCommit();
        var row = await db.SchoolBotScoreOutboxes.SingleAsync(r => r.SchoolBotScoreOutboxId == item.Id, ct);
        if (row.SchoolId != item.SchoolId || row.ScoreExportId != item.ExportId)
            throw new DomainRuleException(ErrorCodes.SchoolMismatch, "Outbox ownership cannot change.");
        row.State = item.State; row.Attempts = item.Attempts; row.LeaseOwner = item.LeaseOwner; row.LeaseUntil = item.LeaseUntil?.UtcDateTime;
        row.NextAttemptAt = item.NextAttemptAt.UtcDateTime; row.ReceiptId = item.ReceiptId; row.ReceiptHash = item.ReceiptHash;
        row.LastErrorCode = item.LastErrorCode; row.UpdateDate = item.UpdateDate.UtcDateTime;
        await db.SaveChangesAsync(ct);
        var siblings = await db.SchoolBotScoreOutboxes.Where(o => o.ScoreExportId == item.ExportId).ToArrayAsync(ct);
        var export = await db.SchoolBotScoreExports.SingleAsync(e => e.SchoolBotScoreExportId == item.ExportId, ct);
        var preview = await GetScorePreviewAsync(export.ScorePreviewId, ct) ?? throw new DomainRuleException(ErrorCodes.NotFound, "Preview not found.");
        export.State = ScoreExportState.Resolve(siblings.Select(o => o.State).ToArray(), preview.Learners.Any(l => l.SkippedCells?.Count > 0),
            siblings.Any(o => o.State == "blocked" && o.LastErrorCode == "SUPERSEDED_BY_PREVIEW"));
        export.UpdateDate = item.UpdateDate.UtcDateTime;
        await unit.AddAuditAsync(audit, item.Id, ct);
        return item;
    }
    private static DateTimeOffset? Offset(DateTime? value) => value is { } date ? Offset(date) : null;
    private static DateTimeOffset Offset(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
