using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Services.Implementations.Assessment;

public sealed class ScoreExportService(ISchoolBotRepositoryScope repository, IAssessmentAuthority authority,
    IGradeDestination destination, TimeProvider clock, IExportPrincipalResolver? principalResolver = null)
{
    public async Task<(int Completed, int Deferred)> SweepAsync(CancellationToken ct = default)
    {
        var now = await repository.GetAssessmentTimeAsync(ct);
        var pending = await repository.ListScoreOutboxAsync(null, 1000, ct);
        var completed = 0;
        var deferred = 0;
        foreach (var exportId in pending.Where(i => i.NextAttemptAt <= now && (i.LeaseUntil is null || i.LeaseUntil <= now))
            .Select(i => i.ExportId).Distinct().Take(50))
        {
            var export = await repository.GetScoreExportAsync(exportId, ct) ?? throw Missing("Export");
            if (export.State == "conflict" && !pending.Any(i => i.ExportId == exportId && i.State is "unknown" or "processing")) continue;
            try
            {
                var principal = await (principalResolver ?? new UnconfiguredExportPrincipalResolver()).ResolveAsync(export, ct);
                completed += await ProcessAsync(principal, exportId, ct);
            }
            catch (DomainRuleException) { deferred++; }
        }
        return (completed, deferred);
    }
    public async Task<object> GetMappingAsync(AssessmentPrincipalContext principal, int runId, CancellationToken ct = default)
    {
        var run = await Authorize(repository, principal, runId, principal.Session.IsSchoolAdmin, ct);
        var slots = await repository.ListGradeSlotsAsync(run.SchoolId, runId, ct);
        var mappings = await repository.ListScoreMappingsAsync(run.SchoolId, runId, ct);
        var candidates = principal.Session.IsSchoolAdmin ? await destination.DiscoverSlotsAsync(principal, run, ct) : [];
        return new { slots, mappings = mappings.Select(m => new { m.Id, m.ActivityId, m.GradeSlotId, m.RetiredAt, etag = OpaqueConcurrencyToken.ToETag(m.UpdateDate) }),
            candidates = candidates.Select(s => new { candidate_key = Hash(s), s.Category, s.TargetSnapshot,
                owned_slot_id = slots.SingleOrDefault(o => o.NGradeId == s.NGradeId && o.AssessmentId == s.AssessmentId && o.ScoreIdentifier == s.ScoreIdentifier && o.RetiredAt is null)?.Id }) };
    }

    public Task<ScoreMappingRecord> SetMappingAsync(AssessmentPrincipalContext principal, int runId,
        string candidateKey, int activityId, string reason, Guid key, string? etag = null, CancellationToken ct = default) =>
        Commit(principal, "set_score_mapping", key, new { runId, candidateKey, activityId, reason, etag }, async (scope, token) =>
        {
            var run = await Authorize(scope, principal, runId, true, token);
            if (activityId <= 0 || string.IsNullOrWhiteSpace(reason) || reason.Length > 1000) QuestionBankValidation.Invalid();
            var candidate = (await destination.DiscoverSlotsAsync(principal, run, token)).SingleOrDefault(s => Hash(s) == candidateKey)
                ?? throw new DomainRuleException("SLOT_UNAVAILABLE", "Select an existing, unambiguous grade slot.");
            if (candidate.SchoolId != run.SchoolId || candidate.CourseRunId != runId) throw new DomainRuleException(ErrorCodes.SchoolMismatch, "Grade context mismatch.");
            var policy = run.CurrentPolicyId is { } policyId ? await scope.GetScoringPolicyAsync(policyId, token) : null;
            if (policy is null || !policy.Policy.ActivityWeights.Any(w => w.ActivityId == activityId))
                throw new DomainRuleException(ErrorCodes.PolicyNotConfigured, "Activity must be part of the scoring policy.");
            var activity = (await TeacherRunService.ActivitiesAsync(scope, run.CurriculumId, token)).SingleOrDefault(a => a.SchoolBotActivityId == activityId);
            if (activity is null || activity.Type == "instruction" || activity.Type == "quiz" && !policy.Policy.QuizCounts)
                throw new DomainRuleException("SOURCE_NOT_EXPORTABLE", "This activity is not an exportable score source.");
            var slots = await scope.ListGradeSlotsAsync(run.SchoolId, runId, token);
            var slot = slots.SingleOrDefault(s => s.NGradeId == candidate.NGradeId && s.AssessmentId == candidate.AssessmentId &&
                s.ScoreIdentifier == candidate.ScoreIdentifier && s.RetiredAt is null);
            await destination.RecheckSlotAsync(principal, run, slot ?? candidate, slot is null, token);
            var now = await scope.GetAssessmentTimeAsync(token);
            slot ??= await scope.WriteGradeSlotAsync(candidate with { Id = 0, OwnershipReason = reason, UpdateDate = now },
                Audit(principal, "GradeSlot.Claimed", "SchoolBotGradeSlot", now, reason), token);
            var mappings = await scope.ListScoreMappingsAsync(run.SchoolId, runId, token);
            var current = mappings.SingleOrDefault(m => m.GradeSlotId == slot.Id && m.RetiredAt is null);
            if (current is not null)
            {
                if ((await scope.ListAuditEventsByEntityAsync("SchoolBotScoreMapping", current.Id, token)).Any(a => a.Event == "Mapping.Locked"))
                    throw new DomainRuleException("MAPPING_LOCKED", "A confirmed mapping is immutable.");
                Match(etag, current.UpdateDate);
                await scope.WriteScoreMappingAsync(current with { RetiredAt = now, UpdateDate = now },
                    Audit(principal, "Mapping.Retired", "SchoolBotScoreMapping", now, reason), token);
            }
            return await scope.WriteScoreMappingAsync(new(0, run.SchoolId, runId, slot.Id, activityId, null, now),
                Audit(principal, "Mapping.Created", "SchoolBotScoreMapping", now, reason), token);
        }, m => m.Id, runId, true, ct);

    public Task<ScorePreviewRecord> PreviewAsync(AssessmentPrincipalContext principal, int runId, Guid key, CancellationToken ct = default) =>
        Commit(principal, "create_score_preview", key, new { runId }, async (scope, token) =>
        {
            var preview = await BuildPreview(scope, principal, runId, token);
            return await scope.WriteScorePreviewAsync(preview, Audit(principal, "ScorePreview.Created", "SchoolBotScorePreview",
                preview.UpdateDate, preview.SnapshotHash), token);
        }, p => p.Id, runId, false, ct);

    public async Task<ScorePreviewRecord> PreviewResolutionAsync(AssessmentPrincipalContext principal, int exportId,
        IReadOnlyList<ScoreResolutionDecision> decisions, Guid key, CancellationToken ct = default)
    {
        var export = await repository.GetScoreExportAsync(exportId, ct) ?? throw Missing("Export");
        var original = await GetPreviewAsync(principal, export.PreviewId, ct);
        return await Commit(principal, "preview_score_resolution", key, new { exportId, decisions }, async (scope, token) =>
        {
            var preview = await BuildResolution(scope, principal, exportId, decisions, token);
            return await scope.WriteScorePreviewAsync(preview, Audit(principal, "ScorePreview.ResolutionCreated",
                "SchoolBotScorePreview", preview.UpdateDate, preview.SnapshotHash), token);
        }, p => p.Id, original.CourseRunId, false, ct);
    }

    public Task<ScoreMappingRecord> RetireMappingAsync(AssessmentPrincipalContext principal, int runId, int mappingId,
        string? etag, Guid key, CancellationToken ct = default) => Commit(principal, "retire_score_mapping", key,
        new { runId, mappingId, etag }, async (scope, token) =>
        {
            var run = await Authorize(scope, principal, runId, true, token);
            var mappings = await scope.ListScoreMappingsAsync(run.SchoolId, runId, token);
            var current = mappings.SingleOrDefault(m => m.Id == mappingId && m.RetiredAt is null) ?? throw Missing("Mapping");
            Match(etag, current.UpdateDate);
            foreach (var history in mappings.Where(m => m.GradeSlotId == current.GradeSlotId))
                if ((await scope.ListAuditEventsByEntityAsync("SchoolBotScoreMapping", history.Id, token)).Any(a => a.Event == "Mapping.Locked"))
                    throw new DomainRuleException("MAPPING_LOCKED", "A confirmed destination retains immutable ownership history.");
            var now = await scope.GetAssessmentTimeAsync(token);
            return await scope.WriteScoreMappingAsync(current with { RetiredAt = now, UpdateDate = Next(now, current.UpdateDate) },
                Audit(principal, "Mapping.Retired", "SchoolBotScoreMapping", now, "School administrator retired an unconfirmed mapping"), token);
        }, m => m.Id, runId, true, ct);

    public async Task<ScorePreviewRecord> GetPreviewAsync(AssessmentPrincipalContext principal, int id, CancellationToken ct = default)
    {
        var preview = await repository.GetScorePreviewAsync(id, ct) ?? throw Missing("Preview");
        await Authorize(repository, principal, preview.CourseRunId, false, ct);
        if (preview.Principal != principal.Session.PrincipalId) throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Preview belongs to a different teacher.");
        return preview;
    }

    public async Task<ScoreExportRecord> ConfirmAsync(AssessmentPrincipalContext principal, int previewId,
        string snapshotHash, string? etag, Guid key, CancellationToken ct = default)
    {
        var initial = await GetPreviewAsync(principal, previewId, ct);
        return await Commit(principal, "confirm_score_export", key, new { previewId, snapshotHash, etag }, async (scope, token) =>
        {
            await destination.RequireVerifiedWriterAsync(token);
            var preview = await scope.GetScorePreviewAsync(previewId, token) ?? throw Missing("Preview");
            Match(etag, preview.UpdateDate);
            var now = await scope.GetAssessmentTimeAsync(token);
            if (preview.ExpiresAt <= now) throw new DomainRuleException("PREVIEW_EXPIRED", "Create a fresh preview.");
            if (preview.State != "preview" || preview.SnapshotHash != snapshotHash) throw new DomainRuleException("SNAPSHOT_MISMATCH", "Preview does not match confirmation.");
            if (preview.Learners.Count == 0 || preview.Learners.Any(l => l.State != "ready"))
                throw new DomainRuleException("PREVIEW_NOT_READY", "Resolve missing sources or locked destinations before confirming.");
            var resolution = Resolution(preview.Context);
            var fresh = resolution is null ? await BuildPreview(scope, principal, preview.CourseRunId, token)
                : await BuildResolution(scope, principal, resolution.Value.ExportId, resolution.Value.Decisions, token);
            if (fresh.SnapshotHash != preview.SnapshotHash) throw new DomainRuleException("SNAPSHOT_STALE", "Sources, roster, mapping or destination changed.");
            await RejectOverlappingExports(scope, preview, token, resolution?.ExportId);
            if (resolution is { } resolved)
            {
                foreach (var prior in (await scope.ListScoreOutboxAsync(resolved.ExportId, 1000, token)).Where(i => i.State is "queued" or "conflict"))
                    await scope.WriteScoreOutboxAsync(prior with { State = "blocked", LastErrorCode = "SUPERSEDED_BY_PREVIEW",
                        LeaseOwner = null, LeaseUntil = null, UpdateDate = Next(now, prior.UpdateDate) },
                        Audit(principal, "ScoreOutbox.Superseded", "SchoolBotScoreOutbox", now, $"preview:{preview.Id}"), token);
            }
            foreach (var mapping in preview.Learners.SelectMany(l => l.Cells).Select(c => c.Mapping).DistinctBy(m => m.Id))
                await scope.WriteScoreMappingAsync(mapping, Audit(principal, "Mapping.Locked", "SchoolBotScoreMapping", now, snapshotHash), token);
            await scope.WriteScorePreviewAsync(preview with { State = "confirmed", UpdateDate = Next(now, preview.UpdateDate) },
                Audit(principal, "ScorePreview.Confirmed", "SchoolBotScorePreview", now, snapshotHash), token);
            var items = preview.Learners.Select(l => new ScoreOutboxRecord(0, preview.SchoolId, 0, l.LearnerId, Guid.NewGuid(),
                Hash(l), l.Cells.Count == 0 ? "skipped" : "queued", 0, null, null, now, null, null,
                l.Cells.Count == 0 ? l.SkippedCells?.FirstOrDefault()?.ReasonCode ?? "SOURCE_RESULT_MISSING" : null, now)).ToArray();
            return await scope.CreateScoreExportAsync(new(0, preview.SchoolId, preview.Id, preview.Principal,
                principal.EmployeeId!.Value, now, "queued", now), items,
                Audit(principal, "ScoreExport.Confirmed", "SchoolBotScoreExport", now, snapshotHash), token);
        }, e => e.Id, initial.CourseRunId, false, ct);
    }

    public async Task<object> GetExportAsync(AssessmentPrincipalContext principal, int id, CancellationToken ct = default)
    {
        var export = await repository.GetScoreExportAsync(id, ct) ?? throw Missing("Export");
        var preview = await repository.GetScorePreviewAsync(export.PreviewId, ct) ?? throw Missing("Preview");
        await Authorize(repository, principal, preview.CourseRunId, false, ct);
        return new { export, items = await repository.ListScoreOutboxAsync(id, 1000, ct) };
    }

    public async Task<int> ProcessAsync(AssessmentPrincipalContext principal, int exportId, CancellationToken ct = default)
    {
        var export = await repository.GetScoreExportAsync(exportId, ct) ?? throw Missing("Export");
        var preview = await GetPreviewAsync(principal, export.PreviewId, ct);
        if (export.ConfirmedBy != principal.Session.PrincipalId || export.EmployeeId != principal.EmployeeId)
            throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Original confirming teacher must remain authorized.");
        var completed = 0;
        foreach (var candidate in (await repository.ListScoreOutboxAsync(exportId, 1000, ct)).Where(i => i.State is "queued" or "processing" or "unknown"))
        {
            var owner = Guid.NewGuid().ToString("N");
            var claimed = await Commit(principal, "claim_score_outbox", Guid.NewGuid(), new { candidate.Id, owner }, async (scope, token) =>
            {
                var current = (await scope.ListScoreOutboxAsync(exportId, 1000, token)).Single(i => i.Id == candidate.Id);
                var parent = await scope.GetScoreExportAsync(exportId, token) ?? throw Missing("Export");
                var now = await scope.GetAssessmentTimeAsync(token);
                if (parent.State == "conflict" && current.State is not ("unknown" or "processing") || current.State == "applied" || current.NextAttemptAt > now || current.LeaseUntil > now) return current;
                return await scope.WriteScoreOutboxAsync(current with { State = "processing", LeaseOwner = owner,
                    LeaseUntil = now.AddMinutes(2), Attempts = current.Attempts + 1, UpdateDate = Next(now, current.UpdateDate) },
                    Audit(principal, "ScoreOutbox.Claimed", "SchoolBotScoreOutbox", now, owner), token);
            }, i => i.Id, preview.CourseRunId, false, ct);
            if (claimed.LeaseOwner != owner) continue;
            try
            {
                // Receipt lookup precedes expiry/source checks: a committed grade must be reconciled after a crash.
                var receipt = await destination.FindReceiptAsync(claimed.OperationKey, claimed.PayloadHash, ct);
                if (receipt is null)
                {
                    if ((await repository.GetScoreExportAsync(exportId, ct))?.State == "conflict")
                        throw new DomainRuleException("GRADE_CONFLICT", "The batch remains stopped after reconciling the unknown operation.");
                    await destination.RequireVerifiedWriterAsync(ct);
                    if (await repository.GetAssessmentTimeAsync(ct) >= preview.ExpiresAt)
                        throw new DomainRuleException("PREVIEW_EXPIRED", "The CAS snapshot expired before apply.");
                    var run = await Authorize(repository, principal, preview.CourseRunId, false, ct);
                    if (!await authority.CanReviewLearnerAsync(principal, run, claimed.LearnerId, ct))
                        throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Learner is outside the current roster.");
                    var learner = preview.Learners.Single(l => l.LearnerId == claimed.LearnerId);
                    var resolution = Resolution(preview.Context);
                    var fresh = resolution is null ? await BuildPreview(repository, principal, preview.CourseRunId, ct)
                        : await BuildResolution(repository, principal, resolution.Value.ExportId, resolution.Value.Decisions, ct, true);
                    if (Hash(fresh.Learners.Single(l => l.LearnerId == claimed.LearnerId)) != Hash(learner) ||
                        AuthorityHash(fresh.Context) != AuthorityHash(preview.Context))
                        throw new DomainRuleException("SNAPSHOT_STALE", "Source, policy, roster or destination changed before apply.");
                    receipt = await destination.ApplyAsync(principal, claimed.OperationKey, claimed.PayloadHash, learner.Cells, ct);
                }
                await Finish(principal, claimed, owner, preview.CourseRunId, receipt, null, ct);
                completed++;
            }
            catch (DomainRuleException ex)
            {
                await Finish(principal, claimed, owner, preview.CourseRunId, null, ex.Code, ct);
                break;
            }
        }
        return completed;
    }

    private Task<ScoreOutboxRecord> Finish(AssessmentPrincipalContext principal, ScoreOutboxRecord claimed, string owner,
        int runId, GradeApplyReceipt? receipt, string? error, CancellationToken ct) => Commit(principal, "finish_score_outbox",
        Guid.NewGuid(), new { claimed.Id, owner, receipt, error }, async (scope, token) =>
        {
            var current = (await scope.ListScoreOutboxAsync(claimed.ExportId, 1000, token)).Single(i => i.Id == claimed.Id);
            if (current.LeaseOwner != owner) throw new DomainRuleException(ErrorCodes.VersionConflict, "Worker lease changed.");
            var now = await scope.GetAssessmentTimeAsync(token);
            var conflict = error is "GRADE_CONFLICT" or "PREVIEW_EXPIRED" or "SNAPSHOT_STALE";
            var unknown = error == "GRADE_WRITE_UNKNOWN" || current.LastErrorCode == "GRADE_WRITE_UNKNOWN" && receipt is null && !conflict;
            return await scope.WriteScoreOutboxAsync(current with { State = receipt is not null ? "applied" : conflict ? "conflict" : unknown ? "unknown" : "queued",
                ReceiptId = receipt?.Id, ReceiptHash = receipt?.Hash, LastErrorCode = unknown ? "GRADE_WRITE_UNKNOWN" : error, LeaseOwner = null, LeaseUntil = null,
                NextAttemptAt = now.AddSeconds(30), UpdateDate = Next(now, current.UpdateDate) },
                Audit(principal, receipt is not null ? "ScoreOutbox.Applied" : "ScoreOutbox.Deferred", "SchoolBotScoreOutbox", now, error ?? receipt!.Hash), token);
        }, i => i.Id, runId, false, ct);

    private async Task<ScorePreviewRecord> BuildPreview(ISchoolBotRepositoryScope scope, AssessmentPrincipalContext principal, int runId,
        CancellationToken ct, IReadOnlyDictionary<int, string>? retained = null)
    {
        var run = await Authorize(scope, principal, runId, false, ct);
        var roster = await authority.GetLearnerIdsAsync(principal, run, ct);
        var ids = roster.Order().ToArray();
        if (ids.Length is 0 or > 1000 || ids.Distinct().Count() != ids.Length || ids.Any(i => i <= 0)) throw new DomainRuleException("ROSTER_UNAVAILABLE", "An authoritative roster of at most 1000 students is required.");
        var mappings = (await scope.ListScoreMappingsAsync(run.SchoolId, runId, ct)).Where(m => m.RetiredAt is null).OrderBy(m => m.Id).ToArray();
        if (mappings.Length == 0) throw new DomainRuleException("MAPPING_REQUIRED", "Configure grade slot mapping first.");
        var slots = (await scope.ListGradeSlotsAsync(run.SchoolId, runId, ct)).Where(s => s.RetiredAt is null).OrderBy(s => s.Id).ToArray();
        foreach (var slot in slots) await destination.RecheckSlotAsync(principal, run, slot, false, ct);
        var targets = await destination.ReadCellsAsync(principal, run, slots, ids, ct);
        var policy = run.CurrentPolicyId is { } policyId ? await scope.GetScoringPolicyAsync(policyId, ct) : null;
        if (policy is null) throw new DomainRuleException(ErrorCodes.PolicyNotConfigured, "Frozen policy is required.");
        var learners = new List<ScorePreviewLearner>();
        var activities = await TeacherRunService.ActivitiesAsync(scope, run.CurriculumId, ct);
        var teacher = new TeacherRunService(scope, scope, authority, clock);
        foreach (var learnerId in ids)
        {
            var total = retained?.ContainsKey(learnerId) == true ? new LearningScoreTotal(runId, learnerId, policy.Id, 0, false, [])
                : await teacher.GetTotalAsync(principal, runId, learnerId, ct);
            var cells = new List<ScorePreviewCell>();
            var errors = new List<string>();
            var skipped = new List<SkippedScoreCell>();
            foreach (var mapping in mappings)
            {
                if (activities.Single(a => a.SchoolBotActivityId == mapping.ActivityId).Type == "quiz" && !policy.Policy.QuizCounts)
                { errors.Add("SOURCE_NOT_EXPORTABLE:" + mapping.ActivityId); continue; }
                var source = total.Sources.SingleOrDefault(s => s.ActivityId == mapping.ActivityId);
                var slot = slots.SingleOrDefault(s => s.Id == mapping.GradeSlotId) ?? throw new DomainRuleException("SLOT_UNAVAILABLE", "Mapping destination is retired.");
                var target = targets.SingleOrDefault(t => t.GradeSlotId == slot.Id && t.LearnerId == learnerId);
                if (retained is not null && retained.TryGetValue(learnerId, out var retainReason))
                {
                    skipped.Add(new(mapping.Id, slot.Id, retainReason, target?.CurrentValue));
                    continue;
                }
                if (target is null) { errors.Add("TARGET_MISSING:" + slot.Id); continue; }
                if (source is null)
                {
                    skipped.Add(new(mapping.Id, slot.Id, total.Gaps?.SingleOrDefault(g => g.ActivityId == mapping.ActivityId)?.ReasonCode
                        ?? "SOURCE_RESULT_MISSING", target.CurrentValue));
                    continue;
                }
                if (target.IsLocked) { errors.Add("TARGET_LOCKED:" + slot.Id); continue; }
                var (raw, integer) = GradeConversion.Convert(source.Earned, source.MaxScore, target.TargetMax);
                cells.Add(new(mapping, slot, target, new(source.Kind, source.Id, source.ActivityId, source.Earned, source.MaxScore,
                    source.UpdateDate, total.PolicyId), raw, integer));
            }
            learners.Add(new(learnerId, errors.Count == 0 ? "ready" : "blocked", errors, cells, skipped));
        }
        var context = CanonicalJson.Serialize(new { run, roster, policy, mappings, slots, learners });
        var now = await scope.GetAssessmentTimeAsync(ct);
        return new(0, run.SchoolId, runId, principal.Session.PrincipalId!, Hash(context), now.AddMinutes(15), "preview", context, learners, now);
    }

    private async Task<ScorePreviewRecord> BuildResolution(ISchoolBotRepositoryScope scope, AssessmentPrincipalContext principal,
        int exportId, IReadOnlyList<ScoreResolutionDecision> decisions, CancellationToken ct, bool alreadySuperseded = false)
    {
        var export = await scope.GetScoreExportAsync(exportId, ct) ?? throw Missing("Export");
        var original = await scope.GetScorePreviewAsync(export.PreviewId, ct) ?? throw Missing("Preview");
        if (export.ConfirmedBy != principal.Session.PrincipalId || export.EmployeeId != principal.EmployeeId ||
            original.SchoolId != export.SchoolId || original.Principal != export.ConfirmedBy || export.SchoolId != principal.Session.SchoolId)
            throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Only the original current confirming teacher can resolve this batch.");
        if (export.State != (alreadySuperseded ? "superseded" : "conflict"))
            throw new DomainRuleException(ErrorCodes.StateConflict, "A conflicted batch must be reconciled before resolution.");
        var items = await scope.ListScoreOutboxAsync(exportId, 1000, ct);
        var now = await scope.GetAssessmentTimeAsync(ct);
        if (items.Count != original.Learners.Count || items.Any(i => i.State is "processing" or "unknown" || i.LeaseUntil > now))
            throw new DomainRuleException("EXPORT_IN_PROGRESS", "An active or unknown write must be reconciled before resolution.");
        var unresolved = items.Where(i => i.State is "queued" or "conflict" || alreadySuperseded && i.State == "blocked" && i.LastErrorCode == "SUPERSEDED_BY_PREVIEW").ToArray();
        if (unresolved.Length == 0 || decisions.Count != unresolved.Length || decisions.Select(d => d.LearnerId).Distinct().Count() != decisions.Count ||
            decisions.Any(d => d.Action is not ("keep" or "replace") || !unresolved.Any(i => i.LearnerId == d.LearnerId)))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Choose keep or replace exactly once for every unresolved learner.");
        var retained = items.Where(i => i.State is "applied" or "skipped").ToDictionary(i => i.LearnerId,
            i => i.State == "applied" ? "SOURCE_ALREADY_APPLIED" : "SOURCE_PREVIOUSLY_SKIPPED");
        foreach (var decision in decisions.Where(d => d.Action == "keep")) retained.Add(decision.LearnerId, "TEACHER_KEEP_EXISTING");
        var preview = await BuildPreview(scope, principal, original.CourseRunId, ct, retained);
        if (!preview.Learners.Select(l => l.LearnerId).Order().SequenceEqual(original.Learners.Select(l => l.LearnerId).Order()))
            throw new DomainRuleException("SNAPSHOT_STALE", "Changed classroom membership requires separate reconciliation.");
        var context = JsonNode.Parse(preview.Context)!.AsObject();
        context["resolution"] = JsonSerializer.SerializeToNode(new { exportId, decisions = decisions.OrderBy(d => d.LearnerId).ToArray() });
        var serialized = CanonicalJson.Serialize(context);
        return preview with { Context = serialized, SnapshotHash = Hash(serialized) };
    }

    private static (int ExportId, IReadOnlyList<ScoreResolutionDecision> Decisions)? Resolution(string context)
    {
        using var json = JsonDocument.Parse(context);
        if (!json.RootElement.TryGetProperty("resolution", out var resolution)) return null;
        return (resolution.GetProperty("exportId").GetInt32(),
            JsonSerializer.Deserialize<ScoreResolutionDecision[]>(resolution.GetProperty("decisions"))
                ?? throw new DomainRuleException(ErrorCodes.StateConflict, "Resolution decisions are missing."));
    }

    private static async Task RejectOverlappingExports(ISchoolBotRepositoryScope scope, ScorePreviewRecord preview,
        CancellationToken ct, int? resolvedExportId = null)
    {
        var requested = preview.Learners.Where(l => l.Cells.Count > 0).ToDictionary(l => l.LearnerId,
            l => l.Cells.Select(c => (c.Slot.NGradeId, c.Slot.AssessmentId, c.Slot.ScoreIdentifier)).ToHashSet());
        if (requested.Count == 0) return;
        var pending = await scope.ListUnresolvedScoreOutboxAsync(preview.SchoolId, requested.Keys.ToArray(), ct);
        foreach (var item in pending)
        {
            if (item.ExportId == resolvedExportId) continue;
            var export = await scope.GetScoreExportAsync(item.ExportId, ct) ?? throw Missing("Pending export");
            var prior = await scope.GetScorePreviewAsync(export.PreviewId, ct) ?? throw Missing("Pending preview");
            if (item.SchoolId != preview.SchoolId || export.SchoolId != preview.SchoolId || prior.SchoolId != preview.SchoolId)
                throw new DomainRuleException(ErrorCodes.SchoolMismatch, "Pending export context is inconsistent.");
            var learner = prior.Learners.SingleOrDefault(l => l.LearnerId == item.LearnerId)
                ?? throw new DomainRuleException(ErrorCodes.StateConflict, "Pending export learner evidence is missing.");
            if (learner.Cells.Any(c => requested[item.LearnerId].Contains((c.Slot.NGradeId, c.Slot.AssessmentId, c.Slot.ScoreIdentifier))))
                throw new DomainRuleException("EXPORT_IN_PROGRESS", "Reconcile the existing export before confirming these learner cells again.");
        }
    }

    private async Task<SchoolBotCourseRunRecord> Authorize(IAssessmentRepository scope, AssessmentPrincipalContext principal, int runId, bool admin, CancellationToken ct)
    {
        var session = principal.Session;
        if (!session.IsAuthConfigured || session.IsSessionExpired || string.IsNullOrWhiteSpace(session.PrincipalId)) throw new DomainRuleException(ErrorCodes.Unauthorized, "Current school session is required.");
        if (session.Channel != "school_web" || principal.ProfileKind != "employee" || principal.EmployeeId is null) throw new DomainRuleException(ErrorCodes.ChannelNotAllowed, "Teacher web profile is required.");
        if (!session.HasActiveSchoolLicense) throw new DomainRuleException(ErrorCodes.LicenseRequired, "Active license is required.");
        var run = await scope.GetCourseRunByIdAsync(runId, ct) ?? throw Missing("Course run");
        if (run.SchoolId != session.SchoolId) throw Missing("Course run");
        if (admin ? !session.IsSchoolAdmin : !await authority.CanReadRunAsync(principal, run, ct))
            throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "School administrator or assigned subject teacher permission is required.");
        return run;
    }
    private async Task<T> Commit<T>(AssessmentPrincipalContext principal, string operation, Guid key, object payload,
        Func<ISchoolBotRepositoryScope, CancellationToken, Task<T>> action, Func<T, int> id, int runId, bool admin, CancellationToken ct)
    {
        if (key == Guid.Empty) QuestionBankValidation.Invalid();
        if (repository is not IIdempotentSchoolBotCommit commit) throw new DomainRuleException(ErrorCodes.ServiceUnavailable, "Atomic export storage is required.");
        var result = await commit.CommitWithIdempotencyAsync(new(checked((int?)principal.Session.SchoolId), principal.Session.PrincipalId!, operation, key, Hash(payload)),
            async (scope, _, _, token) =>
            {
                var current = principal.Session.RecheckAsync is null ? principal.Session : await principal.Session.RecheckAsync(scope, token);
                if (current.PrincipalId != principal.Session.PrincipalId || current.SchoolId != principal.Session.SchoolId) throw new DomainRuleException(ErrorCodes.Unauthorized, "Session changed.");
                await Authorize(scope, principal with { Session = current }, runId, admin, token);
            }, async (scope, _, token) => { var value = await action(scope, token); return new MutationResult<T>(value, 200, id(value), null); }, ct);
        return result.Result.Body;
    }
    private static void Match(string? etag, DateTimeOffset updated)
    {
        if (string.IsNullOrWhiteSpace(etag)) throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match is required.");
        if (!OpaqueConcurrencyToken.Matches(etag, updated)) throw new DomainRuleException(ErrorCodes.VersionConflict, "Preview changed.");
    }
    private static DateTimeOffset Next(DateTimeOffset now, DateTimeOffset previous) => now > previous ? now : previous.AddTicks(10);
    private static string Hash<T>(T value) => CanonicalJson.Hash(value);
    private static string AuthorityHash(string context)
    {
        using var json = JsonDocument.Parse(context);
        return Hash(new[] { "run", "roster", "policy", "mappings", "slots" }.Select(k => json.RootElement.GetProperty(k).GetRawText()).ToArray());
    }
    private static DomainRuleException Missing(string entity) => new(ErrorCodes.NotFound, entity + " not found.");
    private static SchoolBotAuditEventRecord Audit(AssessmentPrincipalContext principal, string action, string entity, DateTimeOffset now, string reason) =>
        new(0, principal.Session.PrincipalId!, action, entity, 0, Guid.NewGuid(), reason, false, principal.EmployeeId, now, principal.EmployeeId, now, checked((int?)principal.Session.SchoolId));
}
