using ServiceSchoolBot.Domain.Projects;

namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface IProjectRepository
{
    Task<ProjectCommitResult> CommitProjectAsync(CommitProjectRequest request, CancellationToken ct = default);
    Task<SchoolBotProjectRecord?> GetProjectByIdAsync(int projectId, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotProjectRecord>> ListProjectsByOwnerAsync(int schoolId, string ownerKind, int ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotProjectRevisionRecord>> ListProjectRevisionsAsync(int projectId, CancellationToken ct = default);
    Task<SchoolBotProjectRevisionRecord?> GetProjectRevisionAsync(int projectId, int revisionNumber, CancellationToken ct = default);
    Task<SchoolBotProjectRecord> SoftDeleteProjectAsync(int projectId, string? expectedETag = null, int? deletedBy = null, CancellationToken ct = default);
}
