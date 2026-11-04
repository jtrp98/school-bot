using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Licensing;

namespace ServiceSchoolBot.Application.Services.Implementations.Licensing;

public sealed partial class LicenseAdminService(
    ILicenseRepository dbAdapter,
    TimeProvider clock,
    IManagementStore? managementStore = null, LicenseAdminService.ReplayState? replayState = null) : ILicenseAdminService
{
    private const string ReplayKeyId = "test-fake-signer-047";
    private static readonly byte[] SeamXorKey = "SchoolBotFakeTestCipherKey202611"u8.ToArray();

    public sealed class ReplayState
    {
        internal ConcurrentDictionary<Guid, LicenseAdminReplayRecord> Entries { get; } = new();
    }

    private readonly ConcurrentDictionary<Guid, LicenseAdminReplayRecord> replayStore = replayState?.Entries ?? new();

    public static LicenseViewV2 ToView(SchoolBotLicenseRecord r) => new(
        Id: r.SchoolBotLicenseId,
        SchoolId: r.SchoolId,
        SchoolName: r.SchoolName,
        KeyHint: r.KeyHint,
        ExpiresAt: r.ExpiresAt.ToUniversalTime(),
        Revoked: r.RevokedAt.HasValue,
        ActivatedDevices: r.CurrentBindingId.HasValue ? 1 : 0,
        UpdateDate: r.UpdateDate?.ToUniversalTime()
    );

    public async Task<IReadOnlyList<LicenseViewV2>> ListLicensesAsync(
        LicenseAdminAuthContext authContext,
        int? schoolId = null,
        CancellationToken ct = default)
    {
        var now = clock.GetUtcNow();
        var auth = LicenseAdminDecisionService.EvaluateAuthorization(authContext, now);
        if (!auth.IsAllowed)
            throw new DomainRuleException(auth.ErrorCode!, auth.Message!);

        var records = await dbAdapter.ListLicensesAsync(schoolId, ct);
        return records.Select(ToView).ToArray();
    }

    public async Task<CreatedLicenseV2Response> CreateLicenseAsync(
        LicenseAdminAuthContext authContext,
        CreateLicenseV2Request request,
        Guid? requestKey = null,
        CancellationToken ct = default)
    {
        var now = clock.GetUtcNow();
        var auth = LicenseAdminDecisionService.EvaluateAuthorization(authContext, now);
        if (!auth.IsAllowed)
            throw new DomainRuleException(auth.ErrorCode!, auth.Message!);

        if (request.SchoolId <= 0)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "SchoolId must be greater than zero.");

        var trimmedName = request.SchoolName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName) || trimmedName.Length is < 1 or > 200)
            throw new DomainRuleException(ErrorCodes.InvalidLicense, "School name must be between 1 and 200 characters.");

        if (request.ExpiresAt <= now)
            throw new DomainRuleException(ErrorCodes.InvalidLicense, "Expiry date must be in the future.");

        if (request.DeviceLimit.HasValue && request.DeviceLimit.Value != 1)
            throw new DomainRuleException(ErrorCodes.InvalidLicense, "Only 1 device per license key is supported.");

        var requestHash = ComputeHash($"{request.SchoolId}:{trimmedName}:{request.ExpiresAt.ToUniversalTime():O}");

        if (requestKey.HasValue)
        {
            if (replayStore.TryGetValue(requestKey.Value, out var storedRecord))
            {
                LicenseCurrentState? currentState = null;
                var existing = await dbAdapter.GetLicenseByIdAsync((int)storedRecord.ResourceId, ct);
                if (existing is not null)
                {
                    currentState = new LicenseCurrentState(existing.SchoolBotLicenseId, existing.RevokedAt.HasValue);
                }

                var decision = LicenseAdminDecisionService.EvaluateReplay(
                    authContext,
                    request.SchoolId,
                    authContext.Principal ?? string.Empty,
                    "create_license",
                    requestKey.Value,
                    requestHash,
                    now,
                    storedRecord,
                    currentState,
                    DecryptSeam);

                if (decision.CleanupAction.Type == LicenseAdminCleanupType.ClearCipherAndKeyId)
                {
                    replayStore[requestKey.Value] = storedRecord with { ResultCipher = null, ResultKeyId = null };
                }

                if (decision.Status == LicenseAdminReplayStatus.Replayed && existing is not null)
                {
                    return new CreatedLicenseV2Response(ToView(existing), decision.ReplayedKey!);
                }

                if (!decision.IsSuccess)
                {
                    throw new DomainRuleException(decision.ErrorCode!, decision.Message!);
                }
            }
        }

        var keyBytes = RandomNumberGenerator.GetBytes(32);
        var key = "SBOT-" + Convert.ToHexStringLower(keyBytes);
        var keyHint = key[..8] + "…" + key[^4..];
        var keyHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(key)));

        var candidate = new SchoolBotLicenseRecord(
            SchoolBotLicenseId: 0,
            SchoolName: trimmedName,
            KeyHash: keyHash,
            KeyHint: keyHint,
            ExpiresAt: request.ExpiresAt.ToUniversalTime(),
            RevokedAt: null,
            CurrentBindingId: null,
            BindingEpoch: 0,
            AllowedNextJkt: null,
            IsDelete: false,
            CreateBy: ParsePrincipalInt(authContext.Principal),
            CreateDate: now,
            UpdateBy: ParsePrincipalInt(authContext.Principal),
            UpdateDate: now,
            SchoolId: request.SchoolId);

        var created = await dbAdapter.CreateLicenseAsync(candidate, ct);

        // Keep in-memory management store in sync so activation / validation endpoints find it
        if (managementStore is not null)
        {
            managementStore.Save(new SchoolLicense(
                created.SchoolBotLicenseId.ToString(),
                created.SchoolName,
                created.KeyHint,
                SHA256.HashData(Encoding.UTF8.GetBytes(key)),
                1,
                created.ExpiresAt,
                false,
                []));
        }

        if (requestKey.HasValue)
        {
            var cipher = EncryptSeam(key);
            var record = new LicenseAdminReplayRecord(
                SchoolId: request.SchoolId,
                Principal: authContext.Principal ?? string.Empty,
                Operation: "create_license",
                RequestKey: requestKey.Value,
                RequestHash: requestHash,
                CompletedAt: now,
                ReplayUntil: now.AddSeconds(900),
                ResourceId: created.SchoolBotLicenseId,
                ResultCipher: cipher,
                ResultKeyId: ReplayKeyId);
            replayStore[requestKey.Value] = record;
        }

        return new CreatedLicenseV2Response(ToView(created), key);
    }

    public async Task<LicenseViewV2> RevokeLicenseAsync(
        LicenseAdminAuthContext authContext,
        int licenseId,
        string? expectedETag = null,
        string? reason = null,
        CancellationToken ct = default)
    {
        var now = clock.GetUtcNow();
        var auth = LicenseAdminDecisionService.EvaluateAuthorization(authContext, now);
        if (!auth.IsAllowed)
            throw new DomainRuleException(auth.ErrorCode!, auth.Message!);

        var revoked = await dbAdapter.RevokeLicenseAsync(
            licenseId,
            ParsePrincipalInt(authContext.Principal),
            expectedETag,
            ct);

        var mgmtLicense = managementStore?.FindLicenseById(licenseId.ToString());
        if (mgmtLicense is not null)
        {
            managementStore!.Save(mgmtLicense with { Revoked = true });
        }

        return ToView(revoked);
    }

    public async Task<LicenseViewV2> UnbindLicenseAsync(
        LicenseAdminAuthContext authContext,
        int licenseId,
        string? expectedETag = null,
        string? nextDeviceJkt = null,
        string? reason = null,
        CancellationToken ct = default)
    {
        var now = clock.GetUtcNow();
        var auth = LicenseAdminDecisionService.EvaluateAuthorization(authContext, now);
        if (!auth.IsAllowed)
            throw new DomainRuleException(auth.ErrorCode!, auth.Message!);

        var updatedBy = ParsePrincipalInt(authContext.Principal);
        var saved = await dbAdapter.CommitLicenseUnbindAsync(new CommitLicenseUnbindRequest(
            licenseId,
            expectedETag,
            nextDeviceJkt,
            updatedBy
        ), ct);

        return ToView(saved);
    }

    private static int? ParsePrincipalInt(string? principal)
    {
        if (string.IsNullOrWhiteSpace(principal)) return null;
        if (int.TryParse(principal, out var id)) return id;
        return Math.Abs(principal.GetHashCode());
    }

    private static string ComputeHash(string input) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(input)));

    private static byte[] EncryptSeam(string plainText)
    {
        var bytes = Encoding.UTF8.GetBytes(plainText);
        var cipher = new byte[bytes.Length];
        for (var i = 0; i < bytes.Length; i++)
        {
            cipher[i] = (byte)(bytes[i] ^ SeamXorKey[i % SeamXorKey.Length]);
        }
        return cipher;
    }

    private static string? DecryptSeam(byte[] cipher, string keyId)
    {
        if (!string.Equals(keyId, ReplayKeyId, StringComparison.Ordinal)) return null;
        var plain = new byte[cipher.Length];
        for (var i = 0; i < cipher.Length; i++)
        {
            plain[i] = (byte)(cipher[i] ^ SeamXorKey[i % SeamXorKey.Length]);
        }
        return Encoding.UTF8.GetString(plain);
    }
}
