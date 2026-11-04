using ServiceSchoolBot.Application.Services.Implementations.Projects;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using System.Security.Cryptography;
using System.Text;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Projects;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly SchoolBotRepositoryState state;

    public ProjectRepository(SchoolBotUnitOfWork unitOfWork)
    {
        state = unitOfWork.State;
    }
    public async Task<ProjectCommitResult> CommitProjectAsync(
        CommitProjectRequest request, CancellationToken ct = default)
    {
        await state.EnsureLoadedAsync(ct);
        await SchoolBotRepositoryState.syncLock.WaitAsync(ct);
        try
        {
            ProjectService.ValidateStoredCommitRequest(request);

            var now = state.clock.GetUtcNow();
            var contentHash = ComputeContentHash(request.Content);
            bool isCreate = !request.ProjectId.HasValue || request.ProjectId.Value <= 0;

            SchoolBotProjectRecord candidateProject;
            SchoolBotProjectRevisionRecord candidateRevision;
            List<SchoolBotProjectRecord> candidateProjects;
            List<SchoolBotProjectRevisionRecord> candidateRevisions;
            int projectIndex = -1;

            if (isCreate)
            {
                if (!string.IsNullOrWhiteSpace(request.ExpectedETag))
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "ExpectedETag must be null when creating a project.");

                var newProjectId = state.nextProjectId;
                var newRevisionId = state.nextRevisionId;
                candidateProject = new SchoolBotProjectRecord(
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

                candidateRevision = new SchoolBotProjectRevisionRecord(
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

                candidateProjects = new List<SchoolBotProjectRecord>(state.projects) { candidateProject };
                candidateRevisions = new List<SchoolBotProjectRevisionRecord>(state.revisions) { candidateRevision };
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.ExpectedETag))
                    throw new DomainRuleException(ErrorCodes.PreconditionRequired, "ExpectedETag is required for project updates.");

                projectIndex = state.projects.FindIndex(p => p.SchoolBotProjectId == request.ProjectId!.Value && !p.IsDelete);
                if (projectIndex < 0)
                    throw new DomainRuleException(ErrorCodes.NotFound, "Project not found.");

                var existing = state.projects[projectIndex];
                if (!OpaqueConcurrencyToken.Matches(request.ExpectedETag, existing.UpdateDate))
                    throw new DomainRuleException(ErrorCodes.VersionConflict, "Concurrency token mismatch.");

                ProjectService.ValidateStoredProjectIdentity(existing, request);

                var existingRevs = state.revisions.Where(r => r.ProjectId == existing.SchoolBotProjectId && !r.IsDelete).ToList();
                var nextRevNumber = (existingRevs.Count > 0 ? existingRevs.Max(r => r.Revision) : 0) + 1;
                var newRevisionId = state.nextRevisionId;

                candidateProject = existing with
                {
                    Name = request.Name,
                    CurrentRevision = nextRevNumber,
                    UpdateBy = request.UserId ?? request.OwnerId,
                    UpdateDate = now
                };

                candidateRevision = new SchoolBotProjectRevisionRecord(
                    SchoolBotProjectRevisionId: newRevisionId,
                    ProjectId: existing.SchoolBotProjectId,
                    Revision: nextRevNumber,
                    Content: request.Content,
                    ContentHash: contentHash,
                    IsDelete: false,
                    CreateBy: request.UserId ?? request.OwnerId,
                    CreateDate: now,
                    UpdateBy: request.UserId ?? request.OwnerId,
                    UpdateDate: now,
                    SchoolId: request.SchoolId);

                candidateProjects = new List<SchoolBotProjectRecord>(state.projects);
                candidateProjects[projectIndex] = candidateProject;
                candidateRevisions = new List<SchoolBotProjectRevisionRecord>(state.revisions) { candidateRevision };
            }

            try
            {
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

            if (isCreate)
            {
                state.projects.Add(candidateProject);
                state.revisions.Add(candidateRevision);
                state.nextProjectId++;
                state.nextRevisionId++;
            }
            else
            {
                state.projects[projectIndex] = candidateProject;
                state.revisions.Add(candidateRevision);
                state.nextRevisionId++;
            }

            return new ProjectCommitResult(candidateProject, candidateRevision);
        }
        finally
        {
            SchoolBotRepositoryState.syncLock.Release();
        }
    }

    private static string ComputeContentHash(string content) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

    public async Task<SchoolBotProjectRecord?> GetProjectByIdAsync(int projectId, CancellationToken ct = default)
    {
        await state.EnsureLoadedAsync(ct);
        await SchoolBotRepositoryState.syncLock.WaitAsync(ct);
        try
        {
            return state.projects.FirstOrDefault(p => p.SchoolBotProjectId == projectId && !p.IsDelete);
        }
        finally { SchoolBotRepositoryState.syncLock.Release(); }
    }

    public async Task<IReadOnlyList<SchoolBotProjectRecord>> ListProjectsByOwnerAsync(
        int schoolId, string ownerKind, int ownerId, CancellationToken ct = default)
    {
        await state.EnsureLoadedAsync(ct);
        await SchoolBotRepositoryState.syncLock.WaitAsync(ct);
        try
        {
            return state.projects.Where(p =>
                p.SchoolId == schoolId &&
                string.Equals(p.OwnerKind, ownerKind, StringComparison.OrdinalIgnoreCase) &&
                p.OwnerId == ownerId &&
                !p.IsDelete).ToArray();
        }
        finally { SchoolBotRepositoryState.syncLock.Release(); }
    }

    public async Task<IReadOnlyList<SchoolBotProjectRevisionRecord>> ListProjectRevisionsAsync(
        int projectId, CancellationToken ct = default)
    {
        await state.EnsureLoadedAsync(ct);
        await SchoolBotRepositoryState.syncLock.WaitAsync(ct);
        try
        {
            return state.revisions.Where(r => r.ProjectId == projectId && !r.IsDelete)
                .OrderBy(r => r.Revision).ToArray();
        }
        finally { SchoolBotRepositoryState.syncLock.Release(); }
    }

    public async Task<SchoolBotProjectRevisionRecord?> GetProjectRevisionAsync(
        int projectId, int revisionNumber, CancellationToken ct = default)
    {
        await state.EnsureLoadedAsync(ct);
        await SchoolBotRepositoryState.syncLock.WaitAsync(ct);
        try
        {
            return state.revisions.FirstOrDefault(r =>
                r.ProjectId == projectId && r.Revision == revisionNumber && !r.IsDelete);
        }
        finally { SchoolBotRepositoryState.syncLock.Release(); }
    }

    public async Task<SchoolBotProjectRecord> SoftDeleteProjectAsync(
        int projectId, string? expectedETag = null, int? deletedBy = null, CancellationToken ct = default)
    {
        await state.EnsureLoadedAsync(ct);
        await SchoolBotRepositoryState.syncLock.WaitAsync(ct);
        try
        {
            var index = state.projects.FindIndex(p => p.SchoolBotProjectId == projectId && !p.IsDelete);
            if (index < 0)
                throw new DomainRuleException(ErrorCodes.NotFound, "Project not found.");

            var existing = state.projects[index];
            if (expectedETag is not null && !OpaqueConcurrencyToken.Matches(expectedETag, existing.UpdateDate))
                throw new DomainRuleException(ErrorCodes.VersionConflict, "Concurrency token mismatch.");

            var now = state.clock.GetUtcNow();
            var updated = existing with
            {
                IsDelete = true,
                UpdateBy = deletedBy ?? existing.OwnerId,
                UpdateDate = now
            };
            var candidateProjects = new List<SchoolBotProjectRecord>(state.projects);
            candidateProjects[index] = updated;
            var candidateRevisions = new List<SchoolBotProjectRevisionRecord>(state.revisions);

            try
            {
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

            state.projects[index] = updated;
            return updated;
        }
        finally { SchoolBotRepositoryState.syncLock.Release(); }
    }

}
