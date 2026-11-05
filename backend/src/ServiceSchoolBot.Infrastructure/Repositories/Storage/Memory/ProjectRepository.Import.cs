using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Projects;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Projects;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed partial class ProjectRepository
{
    public async Task<IdempotentCommitResult<ProjectCommitResult>> CommitGuestImportAsync(
        IdempotencyCommand idempotency,
        CommitProjectRequest request,
        SchoolBotAuditEventRecord audit,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(idempotency);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(audit);
        if (idempotency.SchoolId is <= 0 || string.IsNullOrWhiteSpace(idempotency.Principal) ||
            idempotency.Principal.Length > 512 || string.IsNullOrWhiteSpace(idempotency.Operation) ||
            idempotency.Operation.Length > 200 || idempotency.RequestKey == Guid.Empty ||
            idempotency.RequestHash is not { Length: 64 } ||
            idempotency.RequestHash.Any(c => c is not (>= '0' and <= '9') and not (>= 'a' and <= 'f')))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Invalid idempotency command.");

        await state.EnsureLoadedAsync(ct);
        await SchoolBotRepositoryState.syncLock.WaitAsync(ct);
        try
        {
            var snapshot = await state.CurriculumStore.LoadCurriculumUnitAsync(ct);
            var ledger = snapshot.Idempotency ?? [];
            var stored = ledger.SingleOrDefault(entry =>
                entry.SchoolId == idempotency.SchoolId &&
                string.Equals(entry.Principal, idempotency.Principal, StringComparison.Ordinal) &&
                string.Equals(entry.Operation, idempotency.Operation, StringComparison.Ordinal) &&
                entry.RequestKey == idempotency.RequestKey);

            if (stored is not null)
            {
                if (!string.Equals(stored.RequestHash, idempotency.RequestHash, StringComparison.Ordinal))
                    throw new DomainRuleException(ErrorCodes.IdempotencyConflict, "Payload mismatch for idempotency key.");
                if (state.clock.GetUtcNow() >= stored.ReplayUntil)
                    throw new DomainRuleException(ErrorCodes.IdempotencyExpired, "Idempotency replay window has expired.");
                var replay = CurriculumReplayCipher.Decrypt<ProjectCommitResult>(stored, idempotency);
                return new IdempotentCommitResult<ProjectCommitResult>(replay, true);
            }

            ProjectService.ValidateStoredCommitRequest(request);
            if (request.ProjectId.HasValue && request.ProjectId.Value > 0)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "Guest import must create a new cloud project.");
            if (!string.IsNullOrWhiteSpace(request.ExpectedETag))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "ExpectedETag must be null when importing a guest project.");

            var now = state.clock.GetUtcNow();
            var contentHash = ComputeContentHash(request.Content);
            var newProjectId = state.nextProjectId;
            var newRevisionId = state.nextRevisionId;

            var candidateProject = new SchoolBotProjectRecord(
                SchoolBotProjectId: newProjectId,
                OwnerKind: request.OwnerKind,
                OwnerId: request.OwnerId,
                Mode: request.Mode,
                Name: request.Name,
                CurrentRevision: 1,
                IsDelete: false,
                CreateBy: request.UserId ?? request.OwnerId,
                CreateDate: now,
                UpdateBy: request.UserId ?? request.OwnerId,
                UpdateDate: now,
                SchoolId: request.SchoolId);

            var candidateRevision = new SchoolBotProjectRevisionRecord(
                SchoolBotProjectRevisionId: newRevisionId,
                ProjectId: newProjectId,
                Revision: 1,
                Content: request.Content,
                ContentHash: contentHash,
                IsDelete: false,
                CreateBy: request.UserId ?? request.OwnerId,
                CreateDate: now,
                UpdateBy: request.UserId ?? request.OwnerId,
                UpdateDate: now,
                SchoolId: request.SchoolId);

            var candidateProjects = new List<SchoolBotProjectRecord>(state.projects) { candidateProject };
            var candidateRevisions = new List<SchoolBotProjectRevisionRecord>(state.revisions) { candidateRevision };

            try
            {
                // shortcut: the Development file adapter commits projects before its replay ledger; prove process-crash atomicity with PostgreSQL.
                await state.backingStore.CommitProjectAsync(candidateProjects, candidateRevisions, ct);
            }
            catch
            {
                state.initialized = false;
                try
                {
                    await state.ReloadUnderLockAsync(ct);
                    state.initialized = true;
                }
                catch
                {
                    state.projects.Clear();
                    state.revisions.Clear();
                    state.initialized = false;
                }
                throw;
            }

            state.projects.Add(candidateProject);
            state.revisions.Add(candidateRevision);
            state.nextProjectId++;
            state.nextRevisionId++;

            var commitResult = new ProjectCommitResult(candidateProject, candidateRevision);
            var mutationResult = new MutationResult<ProjectCommitResult>(
                commitResult,
                201,
                candidateProject.SchoolBotProjectId,
                OpaqueConcurrencyToken.ToETag(candidateProject.UpdateDate));
            var encrypted = CurriculumReplayCipher.Encrypt(mutationResult, idempotency);

            await state.CurriculumStore.MutateCurriculumUnitAsync((curr, _) =>
            {
                var currAudits = curr.AuditEvents ?? [];
                var nextAuditId = currAudits.Count == 0 ? 1 : checked(currAudits.Max(a => a.SchoolBotAuditEventId) + 1);
                var auditEntry = audit with
                {
                    SchoolBotAuditEventId = nextAuditId,
                    EntityId = candidateProject.SchoolBotProjectId,
                    CreateDate = now,
                    UpdateDate = now
                };

                var currLedger = curr.Idempotency ?? [];
                var nextIdemId = currLedger.Count == 0 ? 1 : checked(currLedger.Max(e => e.SchoolBotIdempotencyId) + 1);
                var idemEntry = new CurriculumIdempotencyRecord(
                    nextIdemId,
                    idempotency.Principal,
                    idempotency.Operation,
                    idempotency.RequestKey,
                    idempotency.RequestHash,
                    null,
                    mutationResult.HttpStatus,
                    encrypted.Cipher,
                    encrypted.KeyId,
                    mutationResult.ResourceId,
                    now,
                    now.AddHours(24),
                    false,
                    audit.CreateBy,
                    now,
                    audit.UpdateBy,
                    now,
                    idempotency.SchoolId);

                return Task.FromResult(new CurriculumStoreMutation<bool>(
                    curr with
                    {
                        AuditEvents = [.. currAudits, auditEntry],
                        Idempotency = [.. currLedger, idemEntry]
                    },
                    true));
            }, ct);

            return new IdempotentCommitResult<ProjectCommitResult>(mutationResult, false);
        }
        finally
        {
            SchoolBotRepositoryState.syncLock.Release();
        }
    }
}
