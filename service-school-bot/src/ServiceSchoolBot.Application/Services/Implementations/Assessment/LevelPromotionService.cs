using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Services.Implementations.Assessment;

public sealed record LevelPromotionSnapshot(SchoolBotLevelPromotionPolicyRecord PromotionPolicy,
    SchoolBotScoringPolicyRecord ScoringPolicy, int CurrentCurriculumId, int TargetCurriculumId,
    ClassroomLearningContext Context, IReadOnlyList<LearningScoreTotal> Roster, decimal Average, bool Eligible,
    bool MaxLevel, string FormulaVersion);

public sealed class LevelPromotionService(IAssessmentRepository repository, ICurriculumRepository curricula,
    IAssessmentAuthority authority, TimeProvider clock)
{
    public Task<SchoolBotLevelProposalRecord> CreateAsync(AssessmentPrincipalContext principal, int runId,
        Guid key, CancellationToken ct = default) => CommitAsync(principal, runId, "propose_level", key, runId.ToString(),
        async (scope, token) =>
        {
            var teacher = new TeacherRunService(scope, scope, authority, clock);
            var run = await teacher.GetRunAsync(principal, runId, token);
            var previous = await scope.ListLevelProposalsAsync(runId, token);
            if (previous.Any(p => p.State is "promoted" or "retained" or "max_level"))
                throw new DomainRuleException("SUCCESSOR_EXISTS", "Course run already has a finalized level decision.");
            var snapshot = await BuildSnapshotAsync(scope, principal, run, token);
            var now = await scope.GetAssessmentTimeAsync(token);
            var state = !snapshot.Eligible ? "retained" : snapshot.MaxLevel ? "max_level" : "proposed";
            var proposal = new SchoolBotLevelProposalRecord(0, run.SchoolId, runId, snapshot.TargetCurriculumId,
                Hash(snapshot), JsonSerializer.Serialize(snapshot), state, null, null, null, null,
                principal.EmployeeId!.Value, now, now);
            if (state != "proposed")
                proposal = proposal with { SuccessorRunId = await SuccessorAsync(scope, principal, run, snapshot,
                    run.CurriculumId, token), DecisionBy = principal.Session.PrincipalId, DecisionAt = now };
            return await scope.WriteLevelProposalAsync(proposal,
                Audit(principal, run.SchoolId, "LevelProposal.Created", now, "Whole-class level proposal"), token);
        }, ct);

    public async Task<SchoolBotLevelProposalRecord> GetAsync(AssessmentPrincipalContext principal, int id, CancellationToken ct = default)
    {
        var proposal = await repository.GetLevelProposalAsync(id, ct)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Level proposal not found.");
        await new TeacherRunService(repository, curricula, authority, clock).GetRunAsync(principal, proposal.CourseRunId, ct);
        return proposal;
    }

    public async Task<SchoolBotLevelProposalRecord> DecideAsync(AssessmentPrincipalContext principal, int id,
        bool promote, string snapshotHash, string? etag, string? reason, Guid key, CancellationToken ct = default)
    {
        var existing = await GetAsync(principal, id, ct);
        if (!promote && (string.IsNullOrWhiteSpace(reason) || reason.Length > 1000)) QuestionBankValidation.Invalid();
        return await CommitAsync(principal, existing.CourseRunId, promote ? "confirm_level" : "decline_level", key,
            JsonSerializer.Serialize(new { id, promote, snapshotHash, etag, reason }), async (scope, token) =>
            {
                var proposal = await scope.GetLevelProposalAsync(id, token)
                    ?? throw new DomainRuleException(ErrorCodes.NotFound, "Level proposal not found.");
                if (string.IsNullOrWhiteSpace(etag)) throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match is required.");
                if (!OpaqueConcurrencyToken.Matches(etag, proposal.UpdateDate))
                    throw new DomainRuleException(ErrorCodes.VersionConflict, "Level proposal changed.");
                if (proposal.State != "proposed" || proposal.SnapshotHash != snapshotHash)
                    throw new DomainRuleException("PROPOSAL_STALE", "Proposal must be refreshed.");
                var run = await new TeacherRunService(scope, scope, authority, clock).GetRunAsync(principal, proposal.CourseRunId, token);
                var fresh = await BuildSnapshotAsync(scope, principal, run, token);
                if (Hash(fresh) != proposal.SnapshotHash)
                    throw new DomainRuleException("PROPOSAL_STALE", "Roster, policy, term or scores changed.");
                if (promote && (!fresh.Eligible || fresh.MaxLevel))
                    throw new DomainRuleException("LEVEL_NOT_ELIGIBLE", "Class does not qualify for promotion.");
                var others = await scope.ListLevelProposalsAsync(run.SchoolBotCourseRunId, token);
                if (others.Any(p => p.Id != id && p.State is "promoted" or "retained" or "max_level"))
                    throw new DomainRuleException("SUCCESSOR_EXISTS", "Course run already has a successor.");
                var successor = await SuccessorAsync(scope, principal, run, fresh,
                    promote ? fresh.TargetCurriculumId : run.CurriculumId, token);
                var now = await scope.GetAssessmentTimeAsync(token);
                var updated = proposal with { State = promote ? "promoted" : "retained", DecisionBy = principal.Session.PrincipalId,
                    DecisionAt = now, Reason = reason, SuccessorRunId = successor,
                    UpdateDate = now > proposal.UpdateDate ? now : proposal.UpdateDate.AddTicks(10) };
                return await scope.WriteLevelProposalAsync(updated,
                    Audit(principal, run.SchoolId, "LevelProposal.Decided", now, reason ?? "Class promotion confirmed"), token);
            }, ct);
    }

    private async Task<LevelPromotionSnapshot> BuildSnapshotAsync(ISchoolBotRepositoryScope scope,
        AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run, CancellationToken ct)
    {
        if (run.Lifecycle is not ("closing" or "closed")) throw new DomainRuleException("TERM_NOT_ENDED", "Course term has not ended.");
        var context = await authority.GetClassroomContextAsync(principal, run, ct);
        if (context.LearnerIds.Count == 0) throw new DomainRuleException("ROSTER_EMPTY", "Class roster is empty.");
        if (context.LearnerIds.Any(id => id <= 0) || context.LearnerIds.Distinct().Count() != context.LearnerIds.Count ||
            context.NextYear <= 0 || string.IsNullOrWhiteSpace(context.NextTerm) || string.IsNullOrWhiteSpace(context.AuthorityVersion))
            throw new DomainRuleException("NEXT_TERM_NOT_AVAILABLE", "Authoritative next term is unavailable.");
        var curriculum = await scope.GetCurriculumByIdAsync(run.CurriculumId, ct)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Curriculum not found.");
        var policy = await scope.GetPromotionPolicyAsync(curriculum.LevelId, ct)
            ?? throw new DomainRuleException("LEVEL_POLICY_NOT_CONFIGURED", "Level promotion policy is required.");
        if (policy.Threshold is < 0 or > 100) throw new DomainRuleException("LEVEL_POLICY_NOT_CONFIGURED", "Invalid threshold.");
        var scoring = run.CurrentPolicyId is { } policyId ? await scope.GetScoringPolicyAsync(policyId, ct) : null;
        if (scoring is null || scoring.CourseRunId != run.SchoolBotCourseRunId || scoring.SchoolId != run.SchoolId)
            throw new DomainRuleException(ErrorCodes.PolicyNotConfigured, "Frozen scoring policy is required.");
        var levels = await scope.ListLevelsAsync(ct);
        var currentLevel = levels.SingleOrDefault(l => l.Id == curriculum.LevelId)
            ?? throw new DomainRuleException("LEVEL_POLICY_NOT_CONFIGURED", "Current level is unavailable.");
        var next = levels.Where(l => l.Ordinal > currentLevel.Ordinal).OrderBy(l => l.Ordinal).FirstOrDefault();
        var targetId = run.CurriculumId;
        if (next is not null)
        {
            var targets = await scope.ListCurriculaAsync(next.Id, "published", ct);
            if (targets.Count != 1 || targets[0].SchoolId is not null)
                throw new DomainRuleException("NEXT_CURRICULUM_NOT_AVAILABLE", "One unambiguous published next-level curriculum is required.");
            targetId = targets[0].SchoolBotCurriculumId;
        }
        var teacher = new TeacherRunService(scope, scope, authority, clock);
        var totals = new List<LearningScoreTotal>();
        foreach (var learner in context.LearnerIds.Order())
            totals.Add(await teacher.GetTotalAsync(principal, run.SchoolBotCourseRunId, learner, ct));
        var average = totals.Sum(t => t.Total) / totals.Count;
        return new(policy, scoring, run.CurriculumId, targetId, context with { LearnerIds = context.LearnerIds.Order().ToArray() },
            totals, average, average > policy.Threshold, next is null, "weighted-normalized-v1");
    }

    private static async Task<int> SuccessorAsync(ISchoolBotRepositoryScope scope, AssessmentPrincipalContext principal,
        SchoolBotCourseRunRecord run, LevelPromotionSnapshot snapshot, int targetId, CancellationToken ct)
    {
        var runs = await scope.ListCourseRunsAsync(run.SchoolId, 0, ct);
        if (runs.Any(r => r.PreviousRunId == run.SchoolBotCourseRunId || r.ClassroomId == run.ClassroomId &&
            r.NYear == snapshot.Context.NextYear && r.NTerm == snapshot.Context.NextTerm))
            throw new DomainRuleException("SUCCESSOR_EXISTS", "A run already exists for the target classroom and term.");
        var now = await scope.GetAssessmentTimeAsync(ct);
        var successor = run with { SchoolBotCourseRunId = 0, CurriculumId = targetId,
            NYear = snapshot.Context.NextYear, NTerm = snapshot.Context.NextTerm, EffectiveAt = snapshot.Context.EffectiveAt,
            PreviousRunId = run.SchoolBotCourseRunId, CurrentPolicyId = null, Lifecycle = "planned",
            CreateBy = principal.EmployeeId, UpdateBy = principal.EmployeeId, CreateDate = now, UpdateDate = now };
        return (await scope.WriteCourseRunAsync(successor,
            new(0, principal.Session.PrincipalId!, "CourseRun.SuccessorCreated", "SchoolBotCourseRun", 0, Guid.NewGuid(),
                "Whole-class level decision", false, principal.EmployeeId, now, principal.EmployeeId, now, run.SchoolId), ct)).SchoolBotCourseRunId;
    }

    private async Task<SchoolBotLevelProposalRecord> CommitAsync(AssessmentPrincipalContext principal, int runId,
        string operation, Guid key, string payload, Func<ISchoolBotRepositoryScope, CancellationToken, Task<SchoolBotLevelProposalRecord>> mutate, CancellationToken ct)
    {
        if (key == Guid.Empty) QuestionBankValidation.Invalid();
        if (repository is not IIdempotentSchoolBotCommit commit) throw new DomainRuleException(ErrorCodes.ServiceUnavailable, "Atomic storage is required.");
        var command = new IdempotencyCommand(checked((int?)principal.Session.SchoolId), principal.Session.PrincipalId!, operation,
            key, Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(payload))));
        var result = await commit.CommitWithIdempotencyAsync(command, async (scope, _, _, token) =>
        {
            var session = principal.Session.RecheckAsync is null ? principal.Session : await principal.Session.RecheckAsync(scope, token);
            await new TeacherRunService(scope, scope, authority, clock).GetRunAsync(principal with { Session = session }, runId, token);
        }, async (scope, _, token) =>
        {
            var saved = await mutate(scope, token);
            return new MutationResult<SchoolBotLevelProposalRecord>(saved, 200, saved.Id, OpaqueConcurrencyToken.ToETag(saved.UpdateDate));
        }, ct);
        return result.Result.Body;
    }

    private static string Hash(LevelPromotionSnapshot snapshot) => CanonicalJson.Hash(snapshot);
    private static SchoolBotAuditEventRecord Audit(AssessmentPrincipalContext principal, int schoolId, string action,
        DateTimeOffset now, string reason) => new(0, principal.Session.PrincipalId!, action, "SchoolBotLevelProposal", 0,
            Guid.NewGuid(), reason, false, principal.EmployeeId, now, principal.EmployeeId, now, schoolId);
}
