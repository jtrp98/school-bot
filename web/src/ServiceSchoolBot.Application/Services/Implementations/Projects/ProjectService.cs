using ServiceSchoolBot.Application.Common.Model.Projects;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Interfaces.Projects;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Projects;
using System.Buffers;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ServiceSchoolBot.Application.Services.Implementations.Projects;

public sealed class ProjectService(
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

        var createRequest = new CommitProjectRequest(
            ProjectId: null,
            SchoolId: auth.SchoolId!.Value,
            OwnerKind: auth.OwnerKind!,
            OwnerId: auth.OwnerId!.Value,
            Mode: request.Mode.Trim().ToLowerInvariant(),
            Name: request.Name.Trim(),
            Content: contentText,
            ExpectedETag: null,
            UserId: auth.UserId
        );
        // The production import commit atomically stores a new project, revision, audit and replay receipt.
        // Offline synchronization must retain that receipt across a service restart too.
        var commit = idempotencyKey is { } key
            ? (await dbAdapter.CommitGuestImportAsync(new(auth.SchoolId, $"{auth.OwnerKind}:{auth.OwnerId}", operation, key, requestHash),
                createRequest, new(0, $"{auth.OwnerKind}:{auth.OwnerId}", "Project.Created", "SchoolBotProject", 0, Guid.NewGuid(),
                    "Cloud project create", false, auth.UserId, null, auth.UserId, null, auth.SchoolId), ct)).Result.Body
            : await dbAdapter.CommitProjectAsync(createRequest, ct);

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

    public const string GuestImportOperation = "GuestProject.Import";

    public async Task<ImportGuestProjectResponse> ImportGuestProjectAsync(
        ProjectAuthContext auth,
        ImportGuestProjectRequest request,
        string? idempotencyKey,
        Guid? requestId = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(auth);
        ArgumentNullException.ThrowIfNull(request);

        if (!TryParseUuidV4(request.LocalProjectId, out var localProjectGuid))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "local_project_id must be a canonical lowercase UUIDv4.");

        if (!TryParseUuidV4(idempotencyKey, out var headerKeyGuid) || headerKeyGuid != localProjectGuid)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Idempotency-Key must be a valid UUIDv4 matching local_project_id.");

        if (request.LocalRevision < 1)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "local_revision must be >= 1.");

        ValidateMode(request.Mode);
        ValidateName(request.Name);

        var normalizedMode = request.Mode.Trim().ToLowerInvariant();
        var trimmedName = request.Name.Trim();
        var canonicalContent = ValidateAndCanonicalizeImportContent(normalizedMode, request.Content);
        var requestHash = ComputeGuestImportRequestHash(
            localProjectGuid.ToString("D"),
            normalizedMode,
            trimmedName,
            request.LocalRevision,
            canonicalContent);

        var schoolId = auth.SchoolId!.Value;
        var ownerKind = auth.OwnerKind!.Trim().ToLowerInvariant();
        var ownerId = auth.OwnerId!.Value;
        var principal = $"{ownerKind}:{ownerId}";

        var idempotencyCmd = new IdempotencyCommand(
            SchoolId: schoolId,
            Principal: principal,
            Operation: GuestImportOperation,
            RequestKey: localProjectGuid,
            RequestHash: requestHash);

        var commitReq = new CommitProjectRequest(
            ProjectId: null,
            SchoolId: schoolId,
            OwnerKind: ownerKind,
            OwnerId: ownerId,
            Mode: normalizedMode,
            Name: trimmedName,
            Content: canonicalContent,
            ExpectedETag: null,
            UserId: auth.UserId ?? ownerId);

        var audit = new SchoolBotAuditEventRecord(
            SchoolBotAuditEventId: 0,
            Actor: principal,
            Event: GuestImportOperation,
            EntityType: "SchoolBotProject",
            EntityId: 0,
            RequestId: requestId ?? Guid.NewGuid(),
            Reason: $"guest-project:{localProjectGuid:D}",
            IsDelete: false,
            CreateBy: auth.UserId ?? ownerId,
            CreateDate: null,
            UpdateBy: auth.UserId ?? ownerId,
            UpdateDate: null,
            SchoolId: schoolId);

        var committed = await dbAdapter.CommitGuestImportAsync(idempotencyCmd, commitReq, audit, ct);
        var document = ToDocument(committed.Result.Body.Project, committed.Result.Body.Revision);
        return new ImportGuestProjectResponse(document, committed.Replayed);
    }

    public static string ComputeGuestImportRequestHash(
        string localProjectId,
        string mode,
        string name,
        int localRevision,
        string canonicalContentJson)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("content");
            using (var contentDoc = JsonDocument.Parse(canonicalContentJson))
            {
                WriteCanonicalElement(writer, contentDoc.RootElement);
            }
            writer.WriteString("local_project_id", localProjectId.Trim().ToLowerInvariant());
            writer.WriteNumber("local_revision", localRevision);
            writer.WriteString("mode", mode.Trim().ToLowerInvariant());
            writer.WriteString("name", name.Trim());
            writer.WriteEndObject();
        }
        return ComputeSha256(Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    private static bool TryParseUuidV4(string? value, out Guid parsed)
    {
        parsed = Guid.Empty;
        if (value is not { Length: 36 }) return false;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (i is 8 or 13 or 18 or 23)
            {
                if (c != '-') return false;
            }
            else if (c is not ((>= '0' and <= '9') or (>= 'a' and <= 'f')))
            {
                return false;
            }
        }
        if (value[14] != '4') return false;
        if (value[19] is not ('8' or '9' or 'a' or 'b')) return false;
        return Guid.TryParseExact(value, "D", out parsed) && parsed != Guid.Empty;
    }

    private static string ValidateAndCanonicalizeImportContent(string mode, JsonElement content)
    {
        var rawText = ValidateAndSerializeContent(content);
        using var doc = JsonDocument.Parse(rawText);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Project content must be a JSON object.");

        if (!root.TryGetProperty("kind", out var kindProp) || kindProp.ValueKind != JsonValueKind.String ||
            !string.Equals(kindProp.GetString(), mode, StringComparison.Ordinal))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Project content kind must match mode.");

        switch (mode)
        {
            case "code":
                ValidateCodeImportContent(root);
                break;
            case "block":
                ValidateBlockImportContent(root);
                break;
            case "arm":
                ValidateArmImportContent(root);
                break;
            default:
                throw new DomainRuleException(ErrorCodes.InvalidInput, "Unsupported mode.");
        }

        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            WriteCanonicalElement(writer, root);
        }
        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    private static void WriteCanonicalElement(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var prop in element.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(prop.Name);
                    WriteCanonicalElement(writer, prop.Value);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                    WriteCanonicalElement(writer, item);
                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }

    private static void EnsureExactProperties(JsonElement obj, params string[] allowed)
    {
        var set = allowed.ToHashSet(StringComparer.Ordinal);
        var count = 0;
        foreach (var prop in obj.EnumerateObject())
        {
            if (!set.Contains(prop.Name) || prop.Value.ValueKind == JsonValueKind.Null)
                throw new DomainRuleException(ErrorCodes.InvalidInput, $"Unexpected or null property '{prop.Name}'.");
            count++;
        }
        if (count != allowed.Length)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Missing required content properties.");
    }

    private static void ValidateCodeImportContent(JsonElement root)
    {
        EnsureExactProperties(root, "kind", "source");
        var src = root.GetProperty("source");
        if (src.ValueKind != JsonValueKind.String)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "code source must be a string.");
        if (src.GetString()!.Length > 16384)
            throw new DomainRuleException(ErrorCodes.PayloadTooLarge, "code source exceeds 16384 UTF-16 code units.");
    }

    private static void ValidateBlockImportContent(JsonElement root)
    {
        EnsureExactProperties(root, "kind", "stacks");
        var stacks = root.GetProperty("stacks");
        if (stacks.ValueKind != JsonValueKind.Array)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "block stacks must be an array.");

        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        var totalBlocks = 0;
        foreach (var stack in stacks.EnumerateArray())
        {
            if (stack.ValueKind != JsonValueKind.Object)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "stack must be an object.");
            EnsureExactProperties(stack, "x", "y", "blocks");
            if (stack.GetProperty("x").ValueKind != JsonValueKind.Number ||
                stack.GetProperty("y").ValueKind != JsonValueKind.Number)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "stack coordinates must be numbers.");
            var blocks = stack.GetProperty("blocks");
            if (blocks.ValueKind != JsonValueKind.Array)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "stack blocks must be an array.");
            foreach (var blk in blocks.EnumerateArray())
                ValidateBlockElement(blk, seenIds, ref totalBlocks, 0);
        }
    }

    private static void ValidateBlockElement(JsonElement blk, HashSet<string> seenIds, ref int total, int depth)
    {
        if (blk.ValueKind != JsonValueKind.Object)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "block must be an object.");
        total++;
        if (total > 1000)
            throw new DomainRuleException(ErrorCodes.PayloadTooLarge, "block count exceeds 1000.");
        if (depth > 8)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "block nesting depth exceeds 8.");

        if (!blk.TryGetProperty("id", out var idProp) || idProp.ValueKind != JsonValueKind.String ||
            !Guid.TryParseExact(idProp.GetString(), "D", out _) ||
            !seenIds.Add(idProp.GetString()!))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "block id must be a unique canonical UUID.");
        if (!blk.TryGetProperty("enabled", out var enProp) ||
            enProp.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "block enabled must be a boolean.");
        if (!blk.TryGetProperty("type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "block type must be a string.");

        switch (typeProp.GetString())
        {
            case "move":
                EnsureExactProperties(blk, "id", "enabled", "type", "direction", "tiles");
                var dir = blk.GetProperty("direction").GetString();
                if (dir is not ("move_right" or "move_left"))
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "invalid move direction.");
                if (!blk.GetProperty("tiles").TryGetInt32(out var tiles) || tiles is < 1 or > 10)
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "move tiles must be 1..10.");
                break;
            case "jump":
                EnsureExactProperties(blk, "id", "enabled", "type");
                break;
            case "repeat":
                EnsureExactProperties(blk, "id", "enabled", "type", "times", "body");
                if (!blk.GetProperty("times").TryGetInt32(out var times) || times is < 1 or > 10)
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "repeat times must be 1..10.");
                var body = blk.GetProperty("body");
                if (body.ValueKind != JsonValueKind.Array)
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "repeat body must be an array.");
                foreach (var child in body.EnumerateArray())
                    ValidateBlockElement(child, seenIds, ref total, depth + 1);
                break;
            default:
                throw new DomainRuleException(ErrorCodes.InvalidInput, "unsupported block type.");
        }
    }

    private static void ValidateArmImportContent(JsonElement root)
    {
        EnsureExactProperties(root, "kind", "poses", "order", "groups", "joint_names", "time_mode", "loops", "view");
        var timeMode = root.GetProperty("time_mode").GetString();
        if (timeMode is not ("same" or "split"))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "time_mode must be 'same' or 'split'.");
        var view = root.GetProperty("view").GetString();
        if (view is not ("cards" or "table"))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "view must be 'cards' or 'table'.");
        if (!root.GetProperty("loops").TryGetInt32(out var loops) || loops is < 1 or > 9999)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "loops must be 1..9999.");

        var jnames = root.GetProperty("joint_names");
        if (jnames.ValueKind != JsonValueKind.Array || jnames.GetArrayLength() != 6)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "joint_names must contain 6 strings.");
        foreach (var jn in jnames.EnumerateArray())
        {
            if (jn.ValueKind != JsonValueKind.String || jn.GetString()!.Length is < 1 or > 200)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "joint name must be 1..200 chars.");
        }

        var poses = root.GetProperty("poses");
        if (poses.ValueKind != JsonValueKind.Array)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "poses must be an array.");
        var poseIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var p in poses.EnumerateArray())
        {
            if (p.ValueKind != JsonValueKind.Object)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "pose must be an object.");
            EnsureExactProperties(p, "id", "name", "values", "duration_ms", "joint_duration_ms");
            var pid = p.GetProperty("id").GetString();
            if (pid is null || !Guid.TryParseExact(pid, "D", out _) || !poseIds.Add(pid))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "pose id must be a unique canonical UUID.");
            var pname = p.GetProperty("name").GetString();
            if (string.IsNullOrEmpty(pname) || pname.Length > 200)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "pose name must be 1..200 chars.");
            if (!p.GetProperty("duration_ms").TryGetInt32(out var dur) || dur is < 1000 or > 9000)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "pose duration_ms must be 1000..9000.");

            var vals = p.GetProperty("values");
            if (vals.ValueKind != JsonValueKind.Array || vals.GetArrayLength() != 6)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "pose values must have 6 numbers.");
            var vArr = vals.EnumerateArray().Select(x => x.GetDouble()).ToArray();
            if (vArr[0] is < -180 or > 180 ||
                vArr[1] is < -135 or > 135 || vArr[2] is < -135 or > 135 ||
                vArr[3] is < -135 or > 135 || vArr[4] is < -135 or > 135 ||
                vArr[5] is < 0 or > 100)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "pose joint values out of bounds.");

            var jdur = p.GetProperty("joint_duration_ms");
            if (jdur.ValueKind != JsonValueKind.Array || jdur.GetArrayLength() != 6)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "pose joint_duration_ms must have 6 integers.");
            foreach (var d in jdur.EnumerateArray())
            {
                if (!d.TryGetInt32(out var dv) || dv is < 1000 or > 9000)
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "joint_duration_ms out of bounds.");
            }
        }

        var order = root.GetProperty("order");
        if (order.ValueKind != JsonValueKind.Array || order.GetArrayLength() != poseIds.Count)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "order must reference all poses once.");
        var orderSeen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var oid in order.EnumerateArray())
        {
            var s = oid.GetString();
            if (s is null || !poseIds.Contains(s) || !orderSeen.Add(s))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "invalid pose id in order.");
        }

        var groups = root.GetProperty("groups");
        if (groups.ValueKind != JsonValueKind.Array)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "groups must be an array.");
        var groupIds = new HashSet<string>(StringComparer.Ordinal);
        var groupNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var g in groups.EnumerateArray())
        {
            if (g.ValueKind != JsonValueKind.Object)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "group must be an object.");
            EnsureExactProperties(g, "id", "name", "pose_ids");
            var gid = g.GetProperty("id").GetString();
            if (gid is null || !Guid.TryParseExact(gid, "D", out _) || !groupIds.Add(gid))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "group id must be a unique canonical UUID.");
            var gname = g.GetProperty("name").GetString();
            if (string.IsNullOrEmpty(gname) || gname.Length > 20 || !groupNames.Add(gname))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "group name must be unique and 1..20 chars.");
            var pids = g.GetProperty("pose_ids");
            if (pids.ValueKind != JsonValueKind.Array || pids.GetArrayLength() < 2)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "group pose_ids must contain >= 2 pose IDs.");
            foreach (var pid in pids.EnumerateArray())
            {
                if (pid.GetString() is not { } ps || !orderSeen.Contains(ps))
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "group references unknown pose id.");
            }
        }
    }

    public static void ValidateStoredCommitRequest(CommitProjectRequest request)
    {
        if (request.SchoolId <= 0)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "SchoolId must be greater than zero.");
        if (request.OwnerId <= 0)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "OwnerId must be greater than zero.");
        if (!string.Equals(request.OwnerKind, "student", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.OwnerKind, "employee", StringComparison.OrdinalIgnoreCase))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "OwnerKind must be 'student' or 'employee'.");
        if (!string.Equals(request.Mode, "block", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.Mode, "arm", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.Mode, "code", StringComparison.OrdinalIgnoreCase))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Mode must be 'block', 'arm', or 'code'.");
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 200)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Name must be between 1 and 200 characters.");
        if (request.Content is null)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Content cannot be null.");

    }

    public static void ValidateStoredProjectIdentity(
        SchoolBotProjectRecord existing, CommitProjectRequest request)
    {
        if (existing.SchoolId != request.SchoolId ||
            !string.Equals(existing.OwnerKind, request.OwnerKind, StringComparison.OrdinalIgnoreCase) ||
            existing.OwnerId != request.OwnerId ||
            !string.Equals(existing.Mode, request.Mode, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Project tenant, owner, or mode cannot be changed.");
        }

    }
}
