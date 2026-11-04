using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Projects;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Projects;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;
using System.Security.Cryptography;
using System.Text;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed partial class ProjectRepository(SchoolBotDbContext db, SchoolBotUnitOfWork unitOfWork) : IProjectRepository
{
    public async Task<SchoolBotProjectRecord?> GetProjectByIdAsync(
        int projectId, CancellationToken ct = default)
    {
        var row = await db.SchoolBotProjects.AsNoTracking()
            .Where(r => r.SchoolBotProjectId == projectId && !r.IsDelete)
            .SingleOrDefaultAsync(ct);
        return row is null ? null : From(row);
    }

    public async Task<IReadOnlyList<SchoolBotProjectRecord>> ListProjectsByOwnerAsync(
        int schoolId, string ownerKind, int ownerId, CancellationToken ct = default)
    {
        var rows = await db.SchoolBotProjects.AsNoTracking()
            .Where(r => r.SchoolId == schoolId && r.OwnerKind == ownerKind && r.OwnerId == ownerId && !r.IsDelete)
            .ToArrayAsync(ct);
        return rows.Select(From).ToArray();
    }

    public async Task<IReadOnlyList<SchoolBotProjectRevisionRecord>> ListProjectRevisionsAsync(
        int projectId, CancellationToken ct = default)
    {
        var rows = await db.SchoolBotProjectRevisions.AsNoTracking()
            .Where(r => r.ProjectId == projectId && !r.IsDelete)
            .OrderBy(r => r.Revision)
            .ToArrayAsync(ct);
        return rows.Select(From).ToArray();
    }

    public async Task<SchoolBotProjectRevisionRecord?> GetProjectRevisionAsync(
        int projectId, int revisionNumber, CancellationToken ct = default)
    {
        var row = await db.SchoolBotProjectRevisions.AsNoTracking()
            .Where(r => r.ProjectId == projectId && r.Revision == revisionNumber && !r.IsDelete)
            .SingleOrDefaultAsync(ct);
        return row is null ? null : From(row);
    }

    public Task<ProjectCommitResult> CommitProjectAsync(CommitProjectRequest request, CancellationToken ct = default) =>
        unitOfWork.WriteAsync(async () =>
        {
            ProjectService.ValidateStoredCommitRequest(request);
            SchoolBotProject project;
            var now = unitOfWork.Now();
            if (request.ProjectId is null or <= 0)
            {
                if (!string.IsNullOrWhiteSpace(request.ExpectedETag))
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "New projects cannot have an ETag.");
                project = new SchoolBotProject
                {
                    SchoolId = request.SchoolId, OwnerKind = request.OwnerKind, OwnerId = request.OwnerId,
                    Mode = request.Mode, Name = request.Name, CurrentRevision = 1,
                    CreateBy = request.UserId ?? request.OwnerId, CreateDate = now.UtcDateTime,
                    UpdateBy = request.UserId ?? request.OwnerId, UpdateDate = now.UtcDateTime
                };
                db.SchoolBotProjects.Add(project);
                await db.SaveChangesAsync(ct);
            }
            else
            {
                project = await db.SchoolBotProjects.SingleOrDefaultAsync(row =>
                    row.SchoolBotProjectId == request.ProjectId && row.SchoolId == request.SchoolId &&
                    row.OwnerId == request.OwnerId && row.OwnerKind == request.OwnerKind && !row.IsDelete, ct)
                    ?? throw SchoolBotUnitOfWork.Missing("Project");
                if (project.Mode != request.Mode)
                    throw new DomainRuleException(ErrorCodes.StateConflict, "Project mode cannot change.");
                SchoolBotUnitOfWork.Match(request.ExpectedETag, project.UpdateDate, required: true);
                now = unitOfWork.Now(From(project).UpdateDate);
                project.Name = request.Name;
                project.CurrentRevision = checked(project.CurrentRevision + 1);
                project.UpdateBy = request.UserId ?? request.OwnerId;
                project.UpdateDate = now.UtcDateTime;
            }
            var revision = new SchoolBotProjectRevision
            {
                ProjectId = project.SchoolBotProjectId, SchoolId = project.SchoolId,
                Revision = project.CurrentRevision, Content = request.Content,
                ContentHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(request.Content))),
                CreateBy = request.UserId ?? request.OwnerId, CreateDate = now.UtcDateTime,
                UpdateBy = request.UserId ?? request.OwnerId, UpdateDate = now.UtcDateTime
            };
            db.SchoolBotProjectRevisions.Add(revision);
            await db.SaveChangesAsync(ct);
            return new ProjectCommitResult(From(project), From(revision));
        }, ct);

    public Task<SchoolBotProjectRecord> SoftDeleteProjectAsync(
        int projectId, string? expectedETag = null, int? deletedBy = null, CancellationToken ct = default) =>
        unitOfWork.WriteAsync(async () =>
        {
            var row = await db.SchoolBotProjects.SingleOrDefaultAsync(r => r.SchoolBotProjectId == projectId && !r.IsDelete, ct)
                ?? throw SchoolBotUnitOfWork.Missing("Project");
            SchoolBotUnitOfWork.Match(expectedETag, row.UpdateDate);
            row.IsDelete = true;
            row.UpdateBy = deletedBy ?? row.OwnerId;
            row.UpdateDate = unitOfWork.Now(From(row).UpdateDate).UtcDateTime;
            await db.SaveChangesAsync(ct);
            return From(row);
        }, ct);
}
