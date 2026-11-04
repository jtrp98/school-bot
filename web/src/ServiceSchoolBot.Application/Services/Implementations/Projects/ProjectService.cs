using ServiceSchoolBot.Application.Common.Model.Projects;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Interfaces.Projects;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Projects;

namespace ServiceSchoolBot.Application.Services.Implementations.Projects;

public sealed partial class ProjectService(
    IProjectRepository dbAdapter, ProjectService.ReplayState? replayState = null) : IProjectService
{
    private const int MaxContentBytes = 1024 * 1024; // 1 MiB

    internal sealed record ProjectIdempotencyEntry(
        string OwnerKey,
        string Operation,
        string RequestHash,
        CloudProjectDocumentResponse Response);

    public sealed class ReplayState
    {
        internal ConcurrentDictionary<Guid, ProjectIdempotencyEntry> Entries { get; } = new();
    }

    private readonly ConcurrentDictionary<Guid, ProjectIdempotencyEntry> idempotencyStore = replayState?.Entries ?? new();

    private static string ComputeSha256(string input) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(input)));

    private static void ValidateMode(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode) ||
            (!string.Equals(mode, "block", StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(mode, "arm", StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(mode, "code", StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Mode must be 'block', 'arm', or 'code'.");
        }
    }

    private static void ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length is < 1 or > 200)
        {
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Name must be between 1 and 200 characters.");
        }
    }

    private static string ValidateAndSerializeContent(JsonElement content)
    {
        if (content.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Content cannot be null.");
        }

        string rawText;
        if (content.ValueKind == JsonValueKind.String)
        {
            var strVal = content.GetString();
            if (string.IsNullOrWhiteSpace(strVal))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "Content cannot be empty.");
            rawText = strVal;
        }
        else
        {
            rawText = content.GetRawText();
        }

        if (string.IsNullOrWhiteSpace(rawText))
        {
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Content cannot be empty.");
        }

        if (Encoding.UTF8.GetByteCount(rawText) > MaxContentBytes)
        {
            throw new DomainRuleException(ErrorCodes.PayloadTooLarge, "Project content must not exceed 1 MiB.");
        }

        // Validate JSON structure
        try
        {
            using var doc = JsonDocument.Parse(rawText);
        }
        catch (JsonException)
        {
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Project content must be valid JSON.");
        }

        return rawText;
    }

    private static CloudProjectDocumentResponse ToDocument(SchoolBotProjectRecord project, SchoolBotProjectRevisionRecord revision)
    {
        JsonElement parsedContent;
        try
        {
            parsedContent = JsonDocument.Parse(revision.Content).RootElement.Clone();
        }
        catch
        {
            using var emptyDoc = JsonDocument.Parse("{}");
            parsedContent = emptyDoc.RootElement.Clone();
        }

        return new CloudProjectDocumentResponse(
            Schema: 2,
            ProjectId: project.SchoolBotProjectId,
            ProjectRevisionId: revision.SchoolBotProjectRevisionId,
            Mode: project.Mode,
            Name: project.Name,
            Revision: project.CurrentRevision,
            UpdateDate: project.UpdateDate!.Value,
            Content: parsedContent
        );
    }

    public async Task<ProjectListResponse> ListProjectsAsync(
        ProjectAuthContext auth, string? mode, int limit, string? cursor, CancellationToken ct = default)
    {
        if (limit is < 1 or > 100)
            throw new DomainRuleException(ErrorCodes.InvalidQuery, "Limit must be between 1 and 100.");

        string? normalizedMode = null;
        if (!string.IsNullOrWhiteSpace(mode))
        {
            normalizedMode = mode.Trim().ToLowerInvariant();
            ValidateMode(normalizedMode);
        }

        var allProjects = await dbAdapter.ListProjectsByOwnerAsync(
            auth.SchoolId!.Value, auth.OwnerKind!, auth.OwnerId!.Value, ct);

        var query = allProjects.Where(p => !p.IsDelete);
        if (normalizedMode is not null)
        {
            query = query.Where(p => string.Equals(p.Mode, normalizedMode, StringComparison.OrdinalIgnoreCase));
        }

        var items = query
            .OrderBy(p => p.SchoolBotProjectId)
            .Take(limit)
            .Select(p => new ProjectMetadataItem(
                Id: p.SchoolBotProjectId,
                ProjectId: p.SchoolBotProjectId,
                Mode: p.Mode,
                Name: p.Name,
                Revision: p.CurrentRevision,
                UpdateDate: p.UpdateDate!.Value))
            .ToArray();

        return new ProjectListResponse(items, NextCursor: null);
    }

    public async Task<CloudProjectDocumentResponse> CreateProjectAsync(
        ProjectAuthContext auth, CreateProjectRequest request, Guid? idempotencyKey = null, CancellationToken ct = default)
    {
        ValidateMode(request.Mode);
        ValidateName(request.Name);
        var contentText = ValidateAndSerializeContent(request.Content);

        var ownerKey = $"{auth.SchoolId}:{auth.OwnerKind}:{auth.OwnerId}";
        var operation = "create_project";
        var requestHash = ComputeSha256($"{request.Mode.Trim().ToLowerInvariant()}|{request.Name.Trim()}|{contentText}");

        if (idempotencyKey.HasValue && idempotencyStore.TryGetValue(idempotencyKey.Value, out var stored))
        {
            if (!string.Equals(stored.OwnerKey, ownerKey, StringComparison.Ordinal))
            {
                throw new DomainRuleException(ErrorCodes.IdempotencyConflict, "Idempotency key belongs to another principal/owner.");
            }
            if (!string.Equals(stored.Operation, operation, StringComparison.Ordinal) ||
                !string.Equals(stored.RequestHash, requestHash, StringComparison.Ordinal))
            {
                throw new DomainRuleException(ErrorCodes.IdempotencyConflict, "Payload mismatch for idempotency key.");
            }
            return stored.Response;
        }

        var commit = await dbAdapter.CommitProjectAsync(new CommitProjectRequest(
            ProjectId: null,
            SchoolId: auth.SchoolId!.Value,
            OwnerKind: auth.OwnerKind!,
            OwnerId: auth.OwnerId!.Value,
            Mode: request.Mode.Trim().ToLowerInvariant(),
            Name: request.Name.Trim(),
            Content: contentText,
            ExpectedETag: null,
            UserId: auth.UserId
        ), ct);

        var document = ToDocument(commit.Project, commit.Revision);
        if (idempotencyKey.HasValue)
        {
            idempotencyStore[idempotencyKey.Value] = new ProjectIdempotencyEntry(ownerKey, operation, requestHash, document);
        }
        return document;
    }

    public async Task<CloudProjectDocumentResponse> GetProjectAsync(
        ProjectAuthContext auth, int projectId, CancellationToken ct = default)
    {
        if (projectId <= 0)
            throw new DomainRuleException(ErrorCodes.NotFound, "Project not found.");

        var project = await dbAdapter.GetProjectByIdAsync(projectId, ct);
        if (project is null || project.IsDelete)
            throw new DomainRuleException(ErrorCodes.NotFound, "Project not found.");

        if (project.SchoolId != auth.SchoolId!.Value ||
            !string.Equals(project.OwnerKind, auth.OwnerKind, StringComparison.OrdinalIgnoreCase) ||
            project.OwnerId != auth.OwnerId!.Value)
        {
            throw new DomainRuleException(ErrorCodes.NotFound, "Project not found.");
        }

        var revision = await dbAdapter.GetProjectRevisionAsync(projectId, project.CurrentRevision, ct);
        if (revision is null || revision.IsDelete)
            throw new DomainRuleException(ErrorCodes.NotFound, "Project revision not found.");

        return ToDocument(project, revision);
    }

    public async Task<CloudProjectDocumentResponse> UpdateProjectAsync(
        ProjectAuthContext auth, int projectId, UpdateProjectRequest request, string? expectedETag, CancellationToken ct = default)
    {
        if (projectId <= 0)
            throw new DomainRuleException(ErrorCodes.NotFound, "Project not found.");

        if (string.IsNullOrWhiteSpace(expectedETag))
            throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match header is required.");

        ValidateName(request.Name);
        var contentText = ValidateAndSerializeContent(request.Content);

        var existing = await dbAdapter.GetProjectByIdAsync(projectId, ct);
        if (existing is null || existing.IsDelete)
            throw new DomainRuleException(ErrorCodes.NotFound, "Project not found.");

        if (existing.SchoolId != auth.SchoolId!.Value ||
            !string.Equals(existing.OwnerKind, auth.OwnerKind, StringComparison.OrdinalIgnoreCase) ||
            existing.OwnerId != auth.OwnerId!.Value)
        {
            throw new DomainRuleException(ErrorCodes.NotFound, "Project not found.");
        }

        if (!OpaqueConcurrencyToken.Matches(expectedETag, existing.UpdateDate))
        {
            throw new VersionConflictException(existing.UpdateDate, "Concurrency token mismatch.");
        }

        var commit = await dbAdapter.CommitProjectAsync(new CommitProjectRequest(
            ProjectId: projectId,
            SchoolId: auth.SchoolId!.Value,
            OwnerKind: auth.OwnerKind!,
            OwnerId: auth.OwnerId!.Value,
            Mode: existing.Mode,
            Name: request.Name.Trim(),
            Content: contentText,
            ExpectedETag: expectedETag,
            UserId: auth.UserId
        ), ct);

        return ToDocument(commit.Project, commit.Revision);
    }

    public async Task<DeleteProjectResponse> DeleteProjectAsync(
        ProjectAuthContext auth, int projectId, string? expectedETag, CancellationToken ct = default)
    {
        if (projectId <= 0)
            throw new DomainRuleException(ErrorCodes.NotFound, "Project not found.");

        if (string.IsNullOrWhiteSpace(expectedETag))
            throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match header is required.");

        var existing = await dbAdapter.GetProjectByIdAsync(projectId, ct);
        if (existing is null || existing.IsDelete)
            throw new DomainRuleException(ErrorCodes.NotFound, "Project not found.");

        if (existing.SchoolId != auth.SchoolId!.Value ||
            !string.Equals(existing.OwnerKind, auth.OwnerKind, StringComparison.OrdinalIgnoreCase) ||
            existing.OwnerId != auth.OwnerId!.Value)
        {
            throw new DomainRuleException(ErrorCodes.NotFound, "Project not found.");
        }

        if (!OpaqueConcurrencyToken.Matches(expectedETag, existing.UpdateDate))
        {
            throw new VersionConflictException(existing.UpdateDate, "Concurrency token mismatch.");
        }

        var deleted = await dbAdapter.SoftDeleteProjectAsync(projectId, expectedETag, auth.UserId, ct);
        return new DeleteProjectResponse(deleted.SchoolBotProjectId, true, deleted.UpdateDate!.Value);
    }

    public async Task<CloudProjectDocumentResponse> CopyProjectAsync(
        ProjectAuthContext auth, int sourceProjectId, CopyProjectRequest request, string? expectedETag, Guid? idempotencyKey = null, CancellationToken ct = default)
    {
        if (sourceProjectId <= 0)
            throw new DomainRuleException(ErrorCodes.NotFound, "Source project not found.");

        if (string.IsNullOrWhiteSpace(expectedETag))
            throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match header is required.");

        ValidateName(request.Name);

        if (request.SourceProjectRevisionId <= 0)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Invalid source_project_revision_id.");

        var ownerKey = $"{auth.SchoolId}:{auth.OwnerKind}:{auth.OwnerId}";
        var operation = "copy_project";
        var requestHash = ComputeSha256($"{sourceProjectId}|{expectedETag}|{request.Name.Trim()}|{request.SourceProjectRevisionId}");

        if (idempotencyKey.HasValue && idempotencyStore.TryGetValue(idempotencyKey.Value, out var stored))
        {
            if (!string.Equals(stored.OwnerKey, ownerKey, StringComparison.Ordinal))
            {
                throw new DomainRuleException(ErrorCodes.IdempotencyConflict, "Idempotency key belongs to another principal/owner.");
            }
            if (!string.Equals(stored.Operation, operation, StringComparison.Ordinal) ||
                !string.Equals(stored.RequestHash, requestHash, StringComparison.Ordinal))
            {
                throw new DomainRuleException(ErrorCodes.IdempotencyConflict, "Payload mismatch for idempotency key.");
            }
            return stored.Response;
        }

        var sourceProject = await dbAdapter.GetProjectByIdAsync(sourceProjectId, ct);
        if (sourceProject is null || sourceProject.IsDelete)
            throw new DomainRuleException(ErrorCodes.NotFound, "Source project not found.");

        if (sourceProject.SchoolId != auth.SchoolId!.Value ||
            !string.Equals(sourceProject.OwnerKind, auth.OwnerKind, StringComparison.OrdinalIgnoreCase) ||
            sourceProject.OwnerId != auth.OwnerId!.Value)
        {
            throw new DomainRuleException(ErrorCodes.NotFound, "Source project not found.");
        }

        if (!OpaqueConcurrencyToken.Matches(expectedETag, sourceProject.UpdateDate))
        {
            throw new VersionConflictException(sourceProject.UpdateDate, "Concurrency token mismatch on source project.");
        }

        var revisions = await dbAdapter.ListProjectRevisionsAsync(sourceProjectId, ct);
        var sourceRev = revisions.FirstOrDefault(r => r.SchoolBotProjectRevisionId == request.SourceProjectRevisionId && !r.IsDelete);
        if (sourceRev is null)
        {
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Source revision does not belong to source project.");
        }

        var commit = await dbAdapter.CommitProjectAsync(new CommitProjectRequest(
            ProjectId: null,
            SchoolId: auth.SchoolId!.Value,
            OwnerKind: auth.OwnerKind!,
            OwnerId: auth.OwnerId!.Value,
            Mode: sourceProject.Mode,
            Name: request.Name.Trim(),
            Content: sourceRev.Content,
            ExpectedETag: null,
            UserId: auth.UserId
        ), ct);

        var document = ToDocument(commit.Project, commit.Revision);
        if (idempotencyKey.HasValue)
        {
            idempotencyStore[idempotencyKey.Value] = new ProjectIdempotencyEntry(ownerKey, operation, requestHash, document);
        }
        return document;
    }
}
