using System.Buffers;
using System.Text;
using System.Text.Json;
using ServiceSchoolBot.Application.Common.Model.Projects;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Projects;

namespace ServiceSchoolBot.Application.Services.Implementations.Projects;

public sealed partial class ProjectService
{
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
}
