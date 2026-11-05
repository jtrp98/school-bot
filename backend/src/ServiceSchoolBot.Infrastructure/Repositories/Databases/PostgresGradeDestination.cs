using System.Data.Common;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed class PostgresGradeDestination(PGGradeDBPostgreSQLDBContext grades,
    JabjaiSchoolSingleDBPostgreSQLDBContext school, IAssessmentRepository repository, IAssessmentAuthority authority,
    IGradeRepository permissions, IConfiguration configuration, TimeProvider clock,
    GradeExportWriteAuthorization? authorization = null) : IGradeDestination
{
    public Task<IReadOnlyList<GradeSlotRecord>> DiscoverSlotsAsync(AssessmentPrincipalContext principal,
        SchoolBotCourseRunRecord run, CancellationToken ct) => Read<IReadOnlyList<GradeSlotRecord>>(async () =>
    {
        await Access(principal, run, ct);
        var courses = await (from c in school.TplanCourses.AsNoTracking()
            join t in school.TplanCourseTerms.AsNoTracking() on new { c.SchoolId, c.PlanCourseId } equals new { t.SchoolId, t.PlanCourseId }
            join r in school.TplanTermSubLevel2s.AsNoTracking() on new { c.SchoolId, c.PlanId } equals new { r.SchoolId, r.PlanId }
            where c.SchoolId == run.SchoolId && run.PlanCourseIds.Contains(c.PlanCourseId) && c.NYear == run.NYear &&
                c.IsActive == true && !c.CDel && c.CourseStatus == 1 && t.NTerm == run.NTerm && t.IsActive == true && !t.CDel &&
                r.NTermSubLevel2 == run.ClassroomId && r.IsActive == true && !r.CDel
            select new { c.PlanCourseId, c.PlanId, c.SPlaneId }).Distinct().ToArrayAsync(ct);
        if (courses.Length != run.PlanCourseIds.Count) throw Unavailable("Course/term/classroom context is incomplete.");
        var headers = await grades.Pgtgrades.AsNoTracking().Where(g => g.SchoolId == run.SchoolId && !g.CDel &&
            g.NTerm == run.NTerm && g.NTermSubLevel2 == run.ClassroomId).ToArrayAsync(ct);
        var assessments = await grades.Pgtassessments.AsNoTracking().Where(a => a.SchoolId == run.SchoolId && !a.CDel &&
            a.IsActive == true && a.NYear == run.NYear && a.NTerm == run.NTerm && a.NTermSubLevel2 == run.ClassroomId).ToArrayAsync(ct);
        var result = new List<GradeSlotRecord>();
        foreach (var course in courses)
        {
            var matches = headers.Where(h => h.PlanId == course.PlanId && h.SPlaneId == course.SPlaneId).ToArray();
            if (matches.Length != 1) continue;
            var header = matches[0];
            var details = await Details(run.SchoolId, [header.NGradeId], null, false, ct);
            foreach (var assessment in assessments.Where(a => a.NGradeId == header.NGradeId && a.SPlaneId == course.SPlaneId))
            {
                var identifiers = new HashSet<string>(StringComparer.Ordinal);
                foreach (var detail in details)
                    foreach (var cell in LegacyScoreDocument.Scores(LegacyScoreDocument.Parse(detail.Document)))
                        if (cell!["AssessmentId"]!.GetValue<int>() == assessment.AssessmentId)
                            identifiers.Add(cell["ScoreIdentifier"]!.GetValue<string>());
                if (identifiers.Count != 1) continue;
                var identifier = identifiers.Single();
                var category = Category(identifier);
                if (category is null || !decimal.TryParse(assessment.AssessmentMaxScore?.Trim(), NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out var max) || max <= 0) continue;
                var snapshot = CanonicalJson.Serialize(new { run.SchoolId, run.NYear, run.NTerm, run.ClassroomId, course.PlanCourseId,
                    course.PlanId, course.SPlaneId, header.NGradeId, header.DUpdate, header.GradeCloseGrade,
                    header.GradeDicimal, header.MaxBeforeMidTerm, header.MaxAfterMidTerm, header.MaxMidTerm, header.MaxFinalTerm,
                    header.FRatioBeforeMidTerm, header.FRatioAfterMidTerm, header.FRatioMidTerm, header.FRatioLateTerm, header.FRatioQuiz,
                    header.OptionMid, header.OptionFinal, assessment.AssessmentId, assessment.AssessmentMaxScore,
                    assessment.NameIdentifier, assessment.MaxScoreIdentifier, assessment.SubmitPeriod, assessment.UpdatedDate,
                    scoreIdentifier = identifier, targetMax = max });
                result.Add(new(0, run.SchoolId, run.SchoolBotCourseRunId, course.PlanCourseId, course.SPlaneId,
                    header.NGradeId, assessment.AssessmentId, identifier, category, snapshot, "", null,
                    new DateTimeOffset(DateTime.SpecifyKind(assessment.UpdatedDate ?? header.DUpdate, DateTimeKind.Utc))));
            }
        }
        return result.OrderBy(s => s.PlanCourseId).ThenBy(s => s.NGradeId).ThenBy(s => s.AssessmentId).ToArray();
    });

    public Task RecheckSlotAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run,
        GradeSlotRecord slot, bool requireEmpty, CancellationToken ct) => Read(async () =>
    {
        var candidate = (await DiscoverSlotsAsync(principal, run, ct)).SingleOrDefault(s => SameCell(s, slot));
        if (candidate is null || BindingHash(candidate.TargetSnapshot) != BindingHash(slot.TargetSnapshot))
            throw new DomainRuleException("SLOT_UNAVAILABLE", "Existing grade destination changed or is ambiguous.");
        if (requireEmpty)
        {
            if (!Open(candidate)) throw new DomainRuleException("SLOT_UNAVAILABLE", "A locked or unresolved legacy period cannot be claimed.");
            if (!principal.Session.IsSchoolAdmin) throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Only the school administrator can claim a grade slot.");
            var rows = await Details(run.SchoolId, [slot.NGradeId], null, false, ct);
            if (rows.Count == 0) throw Unavailable("The legacy grade details must already exist.");
            foreach (var row in rows)
            {
                var cell = LegacyScoreDocument.Cell(LegacyScoreDocument.Scores(LegacyScoreDocument.Parse(row.Document)), slot.AssessmentId, slot.ScoreIdentifier);
                if (LegacyScoreDocument.Value(cell) is not null)
                    throw new DomainRuleException("SLOT_UNAVAILABLE", "An existing teacher score, including zero, prevents initial SchoolBot ownership.");
            }
        }
        return true;
    });

    public Task<IReadOnlyList<GradeTargetCell>> ReadCellsAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run,
        IReadOnlyList<GradeSlotRecord> slots, IReadOnlyList<int> learnerIds, CancellationToken ct) => Read<IReadOnlyList<GradeTargetCell>>(async () =>
    {
        await Access(principal, run, ct);
        foreach (var id in learnerIds)
            if (!await authority.CanReviewLearnerAsync(principal, run, id, ct))
                throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Learner is outside the current subject roster.");
        foreach (var slot in slots)
            if (slot.SchoolId != run.SchoolId || slot.CourseRunId != run.SchoolBotCourseRunId || !run.PlanCourseIds.Contains(slot.PlanCourseId))
                throw new DomainRuleException(ErrorCodes.SchoolMismatch, "Grade slot context is inconsistent.");
        var rows = await Details(run.SchoolId, slots.Select(s => s.NGradeId).Distinct().ToArray(), learnerIds.ToArray(), false, ct);
        var candidates = await DiscoverSlotsAsync(principal, run, ct);
        var detailIds = rows.Select(r => r.Id).ToArray();
        var edited = await grades.PgtgradeEdits.AsNoTracking().Where(e => e.SchoolId == run.SchoolId && !e.CDel &&
            detailIds.Contains(e.GradeDetailId)).Select(e => e.GradeDetailId).Distinct().ToArrayAsync(ct);
        var settings = await Template(run, ct);
        var cells = new List<GradeTargetCell>();
        foreach (var slot in slots)
        {
            var candidate = candidates.SingleOrDefault(c => SameCell(c, slot)) ?? throw Unavailable("Existing grade slot is no longer available.");
            if (BindingHash(candidate.TargetSnapshot) != BindingHash(slot.TargetSnapshot)) throw Unavailable("Grade destination binding changed.");
            using var context = JsonDocument.Parse(candidate.TargetSnapshot);
            var data = context.RootElement;
            var open = Open(candidate);
            foreach (var group in rows.Where(r => r.GradeId == slot.NGradeId).GroupBy(r => r.LearnerId))
            {
                if (group.Count() != 1) throw Unavailable("Multiple legacy grade details exist for this learner and context.");
                var row = group.Single();
                var document = LegacyScoreDocument.Parse(row.Document);
                var cell = LegacyScoreDocument.Cell(LegacyScoreDocument.Scores(document), slot.AssessmentId, slot.ScoreIdentifier);
                cells.Add(new(row.LearnerId, row.Id, row.Xmin, CanonicalJson.Hash(document), slot.Id,
                    data.GetProperty("targetMax").GetDecimal(), LegacyScoreDocument.Value(cell),
                    CanonicalJson.Hash(new { target = data.Clone(), settings }),
                    !open || settings?.IsLocked == true || edited.Contains(row.Id) || document["getSpecial"]?.GetValue<string>() != "-1"));
            }
        }
        return cells;
    });

    public Task RequireVerifiedWriterAsync(CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        if (authorization is null || string.IsNullOrWhiteSpace(authorization.ApprovedBy) || authorization.ApprovedAt > now ||
            authorization.ExpiresAt <= now || string.IsNullOrWhiteSpace(authorization.WriterEvidence)) throw Gate("WRITER_NOT_VERIFIED");
        if (authorization.CalculatorVersion != LegacyGradeCalculator.Version || string.IsNullOrWhiteSpace(authorization.CalculatorEvidence)) throw Gate("CALCULATOR_NOT_VERIFIED");
        if (string.IsNullOrWhiteSpace(authorization.CoexistenceEvidence)) throw Gate("COEXISTENCE_NOT_VERIFIED");
        if (string.IsNullOrWhiteSpace(authorization.TargetEvidence) || string.IsNullOrWhiteSpace(authorization.AuthorizationEvidence)) throw Gate("WRITER_NOT_VERIFIED");
        if (string.IsNullOrWhiteSpace(authorization.DatabaseEvidence)) throw Gate("RECEIPT_UNAVAILABLE");
        return Task.CompletedTask;
    }

    public Task<GradeApplyReceipt?> FindReceiptAsync(Guid operationKey, string payloadHash, CancellationToken ct) => Read(async () =>
    {
        if (operationKey == Guid.Empty) throw new DomainRuleException(ErrorCodes.InvalidInput, "Operation key is required.");
        if (grades.Database.CurrentTransaction is not null) return await Receipt(operationKey, payloadHash, ct);
        await using var transaction = await grades.Database.BeginTransactionAsync(ct);
        // Waiting for the same operation lock resolves an in-flight commit before treating an absent receipt as retryable.
        await OperationLock(operationKey, ct);
        var receipt = await Receipt(operationKey, payloadHash, ct);
        await transaction.CommitAsync(ct);
        return receipt;
    });

    private async Task<GradeApplyReceipt?> Receipt(Guid operationKey, string payloadHash, CancellationToken ct)
    {
        var key = operationKey.ToString("D");
        var rows = await grades.PgtgradeAuditLogs.AsNoTracking().Where(a => a.Application == "SchoolBot" &&
            a.MethodName == "ScoreExport.Apply" && a.Parameter != null && a.Parameter.Contains(key)).Take(2).ToArrayAsync(ct);
        if (rows.Length == 0) return null;
        if (rows.Length != 1) throw new DomainRuleException(ErrorCodes.IdempotencyConflict, "Duplicate grade receipts require reconciliation.");
        using var json = JsonDocument.Parse(rows[0].Parameter!);
        var root = json.RootElement;
        if (root.GetProperty("operation_key").GetGuid() != operationKey || root.GetProperty("payload_hash").GetString() != payloadHash ||
            root.GetProperty("school_id").GetInt32() != rows[0].SchoolId || root.GetProperty("employee_id").GetInt32() != rows[0].SEmp)
            throw new DomainRuleException(ErrorCodes.IdempotencyConflict, "Grade receipt does not match the operation and confirming teacher.");
        return new GradeApplyReceipt(rows[0].GradeAuditLogId, CanonicalJson.Hash(root));
    }

    public Task<GradeApplyReceipt> ApplyAsync(AssessmentPrincipalContext principal, Guid operationKey, string payloadHash,
        IReadOnlyList<ScorePreviewCell> cells, CancellationToken ct) => Read(async () =>
    {
        await RequireVerifiedWriterAsync(ct);
        if (principal.ProfileKind != "employee" || principal.EmployeeId is not > 0 || principal.Session.Channel != "school_web" ||
            principal.Session.IsSessionExpired || !principal.Session.HasActiveSchoolLicense)
            throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "The current confirming teacher is required.");
        if (operationKey == Guid.Empty || string.IsNullOrWhiteSpace(payloadHash) || cells.Count == 0 || cells.Count > 128 ||
            cells.Select(c => c.Target.LearnerId).Distinct().Count() != 1 || cells.Select(c => c.Slot.SchoolId).Distinct().Count() != 1 ||
            cells.Any(c => c.Slot.SchoolId != principal.Session.SchoolId || c.Target.IsLocked || c.IntegerTarget < 0 || c.IntegerTarget > c.Target.TargetMax))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "One authorized learner's confirmed existing cells are required.");
        foreach (var cell in cells)
        {
            var converted = GradeConversion.Convert(cell.Source.Earned, cell.Source.Max, cell.Target.TargetMax);
            if (cell.Mapping.GradeSlotId != cell.Slot.Id || cell.Mapping.SchoolId != cell.Slot.SchoolId ||
                cell.Mapping.CourseRunId != cell.Slot.CourseRunId || cell.Target.GradeSlotId != cell.Slot.Id ||
                cell.Mapping.ActivityId != cell.Source.ActivityId || converted.Raw != cell.RawTarget || converted.Integer != cell.IntegerTarget)
                throw new DomainRuleException(ErrorCodes.StateConflict, "The confirmed source, mapping and target must match exactly.");
        }
        var schoolId = cells[0].Slot.SchoolId;
        if (!await permissions.VerifyGradeWriterReadyAsync(schoolId, ct)) throw Gate("RECEIPT_UNAVAILABLE");
        await using var transaction = await grades.Database.BeginTransactionAsync(ct);
        var commitAttempted = false;
        try
        {
            await OperationLock(operationKey, ct);
            var existing = await FindReceiptAsync(operationKey, payloadHash, ct);
            if (existing is not null) { await transaction.CommitAsync(ct); return existing; }
            var learnerId = cells[0].Target.LearnerId;
            var runIds = cells.Select(c => c.Slot.CourseRunId).Distinct().ToArray();
            if (runIds.Length != 1) throw new DomainRuleException(ErrorCodes.StateConflict, "Grade cells must belong to one confirmed course run.");
            var run = await repository.GetCourseRunByIdAsync(runIds[0], ct) ?? throw Unavailable("Course run not found.");
            var numberYear = await school.Tyears.AsNoTracking().Where(y => y.SchoolId == schoolId && y.NYear == run.NYear && !y.CDel)
                .Select(y => y.NumberYear).SingleOrDefaultAsync(ct);
            if (numberYear is null or < 2563) throw Gate("CALCULATOR_NOT_VERIFIED");
            var currentSlots = cells.Select(c => c.Slot).DistinctBy(s => s.Id).ToArray();
            var headerIds = currentSlots.Select(s => s.NGradeId).Distinct().ToArray();
            var assessmentIds = currentSlots.Select(s => s.AssessmentId).Distinct().ToArray();
            // Lock existing context rows as well as details; a grade close must not race an accepted write.
            await grades.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM public.\"PGTGrade\" WHERE \"SchoolID\" = {schoolId} AND \"nGradeId\" = ANY({headerIds}) ORDER BY \"nGradeId\" FOR SHARE", ct);
            await grades.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM public.\"PGTAssessment\" WHERE \"SchoolId\" = {schoolId} AND \"AssessmentId\" = ANY({assessmentIds}) ORDER BY \"AssessmentId\" FOR SHARE", ct);
            await grades.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM public.\"PGTGradeSetting\" WHERE \"SchoolID\" = {schoolId} AND \"NYear\" = {run.NYear} AND \"cDel\" = false FOR SHARE", ct);
            foreach (var slot in currentSlots) await RecheckSlotAsync(principal, run, slot, false, ct);
            var fresh = await ReadCellsAsync(principal, run, currentSlots, [learnerId], ct);
            if (cells.Any(c => fresh.SingleOrDefault(f => f.GradeSlotId == c.Slot.Id && f.LearnerId == learnerId) != c.Target))
                throw new DomainRuleException("GRADE_CONFLICT", "Destination values or locks changed after preview.");
            var rows = await Details(schoolId, cells.Select(c => c.Slot.NGradeId).Distinct().ToArray(), [learnerId], true, ct);
            var changes = new List<object>();
            foreach (var group in cells.GroupBy(c => c.Target.GradeDetailId).OrderBy(g => g.Key))
            {
                var row = rows.SingleOrDefault(r => r.Id == group.Key) ?? throw Unavailable("Existing grade detail not found.");
                var before = LegacyScoreDocument.Parse(row.Document);
                if (group.Any(c => c.Target.Xmin != row.Xmin || c.Target.BeforeDocumentHash != CanonicalJson.Hash(before)))
                    throw new DomainRuleException("GRADE_CONFLICT", "Grade detail CAS changed.");
                var scores = (JsonArray)LegacyScoreDocument.Scores(before).DeepClone();
                foreach (var cell in group)
                    LegacyScoreDocument.Cell(scores, cell.Slot.AssessmentId, cell.Slot.ScoreIdentifier)["Score"] = cell.IntegerTarget.ToString(CultureInfo.InvariantCulture);
                var header = await grades.Pgtgrades.AsNoTracking().SingleAsync(g => g.SchoolId == schoolId && g.NGradeId == row.GradeId && !g.CDel, ct);
                var template = await Template(run, ct);
                var math = MathSettings(header, template?.TemplateGrades);
                var derived = LegacyGradeCalculator.Calculate(scores, before, math);
                var stamp = LegacyTimestamp();
                var json = scores.ToJsonString();
                // Let the existing column infer text/json/jsonb instead of assuming a storage migration.
                var scoreData = new NpgsqlParameter("scoreData", NpgsqlDbType.Unknown) { Value = json };
                var count = await grades.Database.ExecuteSqlInterpolatedAsync($"""
                    UPDATE public."PGTGradeDetail" SET "ScoreData" = {scoreData},
                    "ScoreBeforeMidTerm" = {derived["ScoreBeforeMidTerm"]}, "ScoreAfterMidTerm" = {derived["ScoreAfterMidTerm"]},
                    "scoreMidTerm" = {derived["scoreMidTerm"]}, "scoreFinalTerm" = {derived["scoreFinalTerm"]},
                    "scoreBeforeAfterMidTerm" = {derived["scoreBeforeAfterMidTerm"]},
                    "getBeforeQuiz100" = {derived["getBeforeQuiz100"]}, "getAfterQuiz100" = {derived["getAfterQuiz100"]},
                    "getMid100" = {derived["getMid100"]}, "getFinal100" = {derived["getFinal100"]}, "getQuiz100" = {derived["getQuiz100"]},
                    "getScore100" = {derived["getScore100"]}, "getGradeLabel" = {derived["getGradeLabel"]}, "GradeSymbol" = {derived["GradeSymbol"]},
                    "UpdatedBy" = {principal.EmployeeId}, "UpdatedDate" = {stamp}
                    WHERE "SchoolID" = {schoolId} AND "nGradeDetailId" = {row.Id} AND "sID" = {learnerId}
                    AND "nGradeId" = {row.GradeId} AND "cDel" = false AND xmin::text = {row.Xmin}
                    """, ct);
                if (count != 1) throw new DomainRuleException("GRADE_CONFLICT", "Grade detail changed before assignment.");
                var after = (await Details(schoolId, [row.GradeId], [learnerId], false, ct)).Single(r => r.Id == row.Id);
                changes.Add(new { grade_detail_id = row.Id, before_hash = CanonicalJson.Hash(before),
                    after_hash = CanonicalJson.Hash(LegacyScoreDocument.Parse(after.Document)),
                    cells = group.Select(c => new { assessment_id = c.Slot.AssessmentId, identifier = c.Slot.ScoreIdentifier, score = c.IntegerTarget }).ToArray() });
            }
            var receipt = new { operation_key = operationKey, payload_hash = payloadHash, school_id = schoolId,
                employee_id = principal.EmployeeId!.Value, learner_id = learnerId, calculator_version = LegacyGradeCalculator.Version, changes };
            var audit = new PgtgradeAuditLog { SchoolId = schoolId, SEmp = principal.EmployeeId, Application = "SchoolBot",
                MethodName = "ScoreExport.Apply", Parameter = CanonicalJson.Serialize(receipt), Logdate = LegacyTimestamp() };
            grades.PgtgradeAuditLogs.Add(audit);
            await grades.SaveChangesAsync(ct);
            commitAttempted = true;
            await transaction.CommitAsync(ct);
            return new GradeApplyReceipt(audit.GradeAuditLogId, CanonicalJson.Hash(receipt));
        }
        catch
        {
            try { await transaction.RollbackAsync(CancellationToken.None); }
            catch (Exception cleanup) when (cleanup is DbException or InvalidOperationException) { /* Preserve the original commit outcome. */ }
            if (commitAttempted) throw new DomainRuleException("GRADE_WRITE_UNKNOWN", "The commit outcome must be reconciled using the original operation receipt.");
            throw;
        }
        finally { grades.ChangeTracker.Clear(); }
    });

    private async Task<IReadOnlyList<LegacyGradeDetailSnapshot>> Details(int schoolId, int[] gradeIds, int[]? learnerIds,
        bool forUpdate, CancellationToken ct)
    {
        FormattableString query;
        if (forUpdate)
        {
            query = $"""
                SELECT "nGradeDetailId" AS "Id", "nGradeId" AS "GradeId", "sID" AS "LearnerId", "SchoolID" AS "SchoolId",
                xmin::text AS "Xmin", to_jsonb(d)::text AS "Document" FROM public."PGTGradeDetail" d
                WHERE "SchoolID" = {schoolId} AND "nGradeId" = ANY({gradeIds}) AND "sID" = ANY({learnerIds!}) AND "cDel" = false
                ORDER BY "nGradeDetailId" LIMIT 10001 FOR UPDATE
                """;
        }
        else if (learnerIds is not null)
        {
            query = $"""
                SELECT "nGradeDetailId" AS "Id", "nGradeId" AS "GradeId", "sID" AS "LearnerId", "SchoolID" AS "SchoolId",
                xmin::text AS "Xmin", to_jsonb(d)::text AS "Document" FROM public."PGTGradeDetail" d
                WHERE "SchoolID" = {schoolId} AND "nGradeId" = ANY({gradeIds}) AND "sID" = ANY({learnerIds}) AND "cDel" = false
                ORDER BY "nGradeDetailId" LIMIT 10001
                """;
        }
        else
        {
            query = $"""
                SELECT "nGradeDetailId" AS "Id", "nGradeId" AS "GradeId", "sID" AS "LearnerId", "SchoolID" AS "SchoolId",
                xmin::text AS "Xmin", to_jsonb(d)::text AS "Document" FROM public."PGTGradeDetail" d
                WHERE "SchoolID" = {schoolId} AND "nGradeId" = ANY({gradeIds}) AND "cDel" = false
                ORDER BY "nGradeDetailId" LIMIT 10001
                """;
        }
        var rows = await grades.Database.SqlQuery<LegacyGradeDetailSnapshot>(query).ToArrayAsync(ct);
        if (rows.Length > 10000) throw Unavailable("The legacy grade context exceeds the bounded read size.");
        return rows;
    }

    private Task<int> OperationLock(Guid operationKey, CancellationToken ct)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes("schoolbot-grade:" + operationKey.ToString("D")));
        var key = BitConverter.ToInt64(bytes, 0);
        return grades.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({key})", ct);
    }

    private async Task<PgtgradeSetting?> Template(SchoolBotCourseRunRecord run, CancellationToken ct)
    {
        var levels = await (from room in school.TtermSubLevel2s.AsNoTracking()
            join level in school.TsubLevels.AsNoTracking() on new { room.SchoolId, room.NTsubLevel } equals new { level.SchoolId, level.NTsubLevel }
            where room.SchoolId == run.SchoolId && room.NTermSubLevel2 == run.ClassroomId && !room.CDel && !level.CDel
            select level.NTlevel).Distinct().ToArrayAsync(ct);
        if (levels.Length != 1 || levels[0] is null) throw Unavailable("Class-level grade template context is ambiguous.");
        var settings = await grades.PgtgradeSettings.AsNoTracking().Where(s => s.SchoolId == run.SchoolId && s.Nyear == run.NYear &&
            s.ClassLevelId == levels[0] && !s.CDel).Take(2).ToArrayAsync(ct);
        if (settings.Length > 1) throw Unavailable("Multiple active grade templates require resolution.");
        return settings.SingleOrDefault();
    }

    private async Task Access(AssessmentPrincipalContext p, SchoolBotCourseRunRecord r, CancellationToken ct)
    {
        if (r.IsDelete || p.Session.SchoolId != r.SchoolId || !p.Session.IsAuthConfigured || p.Session.IsSessionExpired ||
            !p.Session.HasActiveSchoolLicense || p.Session.Channel != "school_web" || p.ProfileKind != "employee" || p.EmployeeId is not > 0 ||
            !p.Session.IsSchoolAdmin && !await authority.CanReadRunAsync(p, r, ct))
            throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Current school administrator or assigned teacher context is required.");
    }
    private static bool SameCell(GradeSlotRecord a, GradeSlotRecord b) => a.SchoolId == b.SchoolId && a.CourseRunId == b.CourseRunId &&
        a.PlanCourseId == b.PlanCourseId && a.CourseId == b.CourseId && a.NGradeId == b.NGradeId &&
        a.AssessmentId == b.AssessmentId && a.ScoreIdentifier == b.ScoreIdentifier;
    private static bool Open(GradeSlotRecord slot)
    {
        using var json = JsonDocument.Parse(slot.TargetSnapshot);
        var root = json.RootElement;
        return root.GetProperty("GradeCloseGrade").ValueKind == JsonValueKind.String && root.GetProperty("GradeCloseGrade").GetString() == "0" &&
            root.GetProperty("SubmitPeriod").ValueKind == JsonValueKind.String && root.GetProperty("SubmitPeriod").GetString() == "0";
    }
    private static string BindingHash(string snapshot)
    {
        using var json = JsonDocument.Parse(snapshot);
        return CanonicalJson.Hash(new[] { "SchoolId", "NYear", "NTerm", "ClassroomId", "PlanCourseId", "PlanId", "SPlaneId", "NGradeId", "AssessmentId", "scoreIdentifier" }
            .Select(name => json.RootElement.GetProperty(name).Clone()).ToArray());
    }
    private static string? Category(string identifier)
    {
        foreach (var (prefix, max, name) in new[] { ("scoreGrade", 20, "before_mid"), ("scoreCheewat", 20, "after_mid"), ("scoreMid", 10, "mid"), ("scoreFinal", 10, "final") })
            if (identifier.StartsWith(prefix, StringComparison.Ordinal) && int.TryParse(identifier.AsSpan(prefix.Length), NumberStyles.None,
                CultureInfo.InvariantCulture, out var n) && n >= 1 && n <= max && identifier == prefix + n.ToString(CultureInfo.InvariantCulture)) return name;
        return null;
    }
    private static LegacyGradeMathSettings MathSettings(Pgtgrade h, string? template)
    {
        decimal Max(string? value) => decimal.TryParse(value?.Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var n) && n >= 0
            ? n : throw Gate("CALCULATOR_NOT_VERIFIED");
        if (h.GradeDicimal is not ("0" or "1")) throw Gate("CALCULATOR_NOT_VERIFIED");
        return new(Max(h.MaxBeforeMidTerm), Max(h.MaxAfterMidTerm), Max(h.MaxMidTerm), Max(h.MaxFinalTerm),
            Convert.ToDecimal(h.FRatioBeforeMidTerm ?? 0), Convert.ToDecimal(h.FRatioAfterMidTerm ?? 0),
            Convert.ToDecimal(h.FRatioMidTerm), Convert.ToDecimal(h.FRatioLateTerm), Convert.ToDecimal(h.FRatioQuiz), h.GradeDicimal == "1" ? 2 : 0, template);
    }
    private DateTime LegacyTimestamp()
    {
        var name = configuration["SchoolBot:LegacyAuthority:TimeZoneId"];
        if (string.IsNullOrWhiteSpace(name)) throw Gate("WRITER_NOT_VERIFIED");
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById(name); }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException) { throw Gate("WRITER_NOT_VERIFIED"); }
        return DateTime.SpecifyKind(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), zone).DateTime, DateTimeKind.Unspecified);
    }
    private static async Task<T> Read<T>(Func<Task<T>> read)
    {
        try { return await read(); }
        catch (DbException) { throw Unavailable("Legacy grade storage is unavailable."); }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException or OverflowException)
        { throw new DomainRuleException("TARGET_NOT_READY", "Legacy grade data is malformed or unsupported."); }
    }
    private static DomainRuleException Gate(string code) => new(code, "Current approved legacy writer, calculator, coexistence and receipt evidence is required.");
    private static DomainRuleException Unavailable(string message) => new("GRADE_SOURCE_UNAVAILABLE", message);
}
