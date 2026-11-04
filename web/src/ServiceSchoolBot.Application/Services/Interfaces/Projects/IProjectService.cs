using ServiceSchoolBot.Application.Common.Model.Projects;
namespace ServiceSchoolBot.Application.Services.Interfaces.Projects;

public interface IProjectService
{
    Task<ProjectListResponse> ListProjectsAsync(
        ProjectAuthContext auth, string? mode, int limit, string? cursor, CancellationToken ct = default);

    Task<CloudProjectDocumentResponse> CreateProjectAsync(
        ProjectAuthContext auth, CreateProjectRequest request, Guid? idempotencyKey = null, CancellationToken ct = default);

    Task<CloudProjectDocumentResponse> GetProjectAsync(
        ProjectAuthContext auth, int projectId, CancellationToken ct = default);

    Task<CloudProjectDocumentResponse> UpdateProjectAsync(
        ProjectAuthContext auth, int projectId, UpdateProjectRequest request, string? expectedETag, CancellationToken ct = default);

    Task<DeleteProjectResponse> DeleteProjectAsync(
        ProjectAuthContext auth, int projectId, string? expectedETag, CancellationToken ct = default);

    Task<CloudProjectDocumentResponse> CopyProjectAsync(
        ProjectAuthContext auth, int sourceProjectId, CopyProjectRequest request, string? expectedETag, Guid? idempotencyKey = null, CancellationToken ct = default);

    Task<ImportGuestProjectResponse> ImportGuestProjectAsync(
        ProjectAuthContext auth, ImportGuestProjectRequest request, string? idempotencyKey, Guid? requestId = null, CancellationToken ct = default);
}
