using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Licensing;

namespace ServiceSchoolBot.Application.Services.Implementations.Licensing;

public sealed partial class LicenseDeviceService : ILicenseDeviceService
{
    private readonly ILicenseRepository dbAdapter;
    private readonly ILicenseTokenSigner signer;
    private readonly TimeProvider clock;
    private readonly IManagementStore? managementStore;

    public sealed class ReplayState
    {
        internal ConcurrentDictionary<string, SemaphoreSlim> Locks { get; } = new();
        internal ConcurrentDictionary<string, DateTimeOffset> Nonces { get; } = new();
        internal ConcurrentDictionary<(string DeviceJkt, string KeyHash, string RequestKey), (string RequestHash, EntitlementResult Result, DateTimeOffset ReplayUntil)> Entries { get; } = new();
    }

    private readonly ConcurrentDictionary<string, SemaphoreSlim> licenseLocks;
    private readonly ConcurrentDictionary<string, DateTimeOffset> consumedNonces;
    private readonly ConcurrentDictionary<(string DeviceJkt, string KeyHash, string RequestKey), (string RequestHash, EntitlementResult Result, DateTimeOffset ReplayUntil)> idempotencyStore;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public LicenseDeviceService(
        ILicenseRepository dbAdapter,
        ILicenseTokenSigner signer,
        TimeProvider? clock = null,
        IManagementStore? managementStore = null, ReplayState? replayState = null)
    {
        this.dbAdapter = dbAdapter;
        this.signer = signer;
        this.clock = clock ?? TimeProvider.System;
        this.managementStore = managementStore;
        var state = replayState ?? new ReplayState();
        licenseLocks = state.Locks;
        consumedNonces = state.Nonces;
        idempotencyStore = state.Entries;
    }

    private bool TryConsumeNonce(string nonce)
    {
        var now = clock.GetUtcNow();
        // Prune expired nonces
        if (consumedNonces.Count > 1000)
        {
            foreach (var kv in consumedNonces)
            {
                if (kv.Value <= now) consumedNonces.TryRemove(kv.Key, out _);
            }
        }
        return consumedNonces.TryAdd(nonce, now.AddMinutes(5));
    }

    public Task<DeviceChallengeResponse> CreateChallengeAsync(CreateChallengeRequest request, CancellationToken ct = default)
    {
        if (request is null || request.DeviceJwk is null)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Device JWK is required.");

        var deviceJkt = DeviceProofVerifier.ComputeDeviceJkt(request.DeviceJwk);
        var now = clock.GetUtcNow();
        var expiresAt = now.AddSeconds(120);
        var nonce = Guid.NewGuid().ToString("D");

        var payload = new
        {
            device_jkt = deviceJkt,
            nonce,
            iat = now.ToUnixTimeSeconds(),
            exp = expiresAt.ToUnixTimeSeconds()
        };

        var challenge = signer.Sign("JWT", payload);
        return Task.FromResult(new DeviceChallengeResponse(challenge, now.ToUniversalTime(), expiresAt.ToUniversalTime()));
    }

    public async Task<EntitlementResult> ActivateAsync(
        ActivateLicenseRequest request,
        string rawJsonBody,
        string? idempotencyKey = null,
        CancellationToken ct = default)
    {
        if (request is null)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Key) || request.Key.Length > 256)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "A valid license key is required.");

        if (request.DeviceJwk is null)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Device JWK is required.");

        if (string.IsNullOrWhiteSpace(request.Challenge))
            throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Challenge is required.");

        if (string.IsNullOrWhiteSpace(request.Proof))
            throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Device proof is required.");

        var deviceJkt = DeviceProofVerifier.ComputeDeviceJkt(request.DeviceJwk);
        var now = clock.GetUtcNow();

        var keyHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(request.Key)));
        var keyLock = licenseLocks.GetOrAdd(keyHash, _ => new SemaphoreSlim(1, 1));

        await keyLock.WaitAsync(ct);
        try
        {
            var requestHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawJsonBody)));

            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                if (idempotencyStore.TryGetValue((deviceJkt, keyHash, idempotencyKey), out var stored))
                {
                    if (!string.Equals(stored.RequestHash, requestHash, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new DomainRuleException(ErrorCodes.IdempotencyConflict, "Payload mismatch for idempotency key.");
                    }

                    if (now >= stored.ReplayUntil)
                        throw new DomainRuleException(ErrorCodes.IdempotencyExpired, "Replay window expired.");

                    var existingLicense = await dbAdapter.GetLicenseByKeyHashAsync(keyHash, ct);
                    if (existingLicense is null || existingLicense.RevokedAt.HasValue || existingLicense.ExpiresAt <= now)
                    {
                        throw new DomainRuleException(ErrorCodes.LicenseInactive, "License expired or revoked.");
                    }

                    if (existingLicense.CurrentBindingId != stored.Result.BindingId)
                    {
                        throw new DomainRuleException(ErrorCodes.DeviceNotActivated, "Binding is no longer active.");
                    }

                    return stored.Result;
                }
            }

            DeviceProofVerifier.VerifyServerChallenge(request.Challenge, signer, now, TryConsumeNonce, deviceJkt);

            var expectedBodyHash = DeviceProofVerifier.ComputeBodyHashWithoutProof(rawJsonBody);
            DeviceProofVerifier.VerifyDeviceProof(
                request.Proof,
                request.DeviceJwk,
                request.Challenge,
                "POST",
                "/api/service-school-bot/licenses/activate",
                idempotencyKey,
                expectedBodyHash);

            var license = await dbAdapter.GetLicenseByKeyHashAsync(keyHash, ct)
                ?? throw new DomainRuleException(ErrorCodes.LicenseInvalid, "License key not found.");

            if (license.RevokedAt.HasValue || license.ExpiresAt <= now)
                throw new DomainRuleException(ErrorCodes.LicenseInactive, "License expired or revoked.");

            var expiresAt = MinTime(now.AddSeconds(259200), license.ExpiresAt);

            if (license.CurrentBindingId.HasValue)
            {
                var currentBinding = await dbAdapter.GetBindingByIdAsync(license.CurrentBindingId.Value, ct);
                if (currentBinding is not null && string.Equals(currentBinding.DeviceJkt, deviceJkt, StringComparison.OrdinalIgnoreCase))
                {
                    var updatedBinding = currentBinding with
                    {
                        LastCheckedAt = now,
                        LastEntitlementExpiresAt = expiresAt,
                        UpdateDate = now
                    };
                    EntitlementResult? prepared = null;
                    await dbAdapter.UpdateBindingAsync(updatedBinding, ct,
                        commit => prepared = PrepareEntitlement(commit));
                    var res = prepared ?? throw new InvalidOperationException("Entitlement was not prepared.");

                    if (!string.IsNullOrWhiteSpace(idempotencyKey))
                    {
                        idempotencyStore[(deviceJkt, keyHash, idempotencyKey)] = (requestHash, res, now.AddSeconds(900));
                    }
                    return res;
                }

                throw new DomainRuleException(ErrorCodes.DeviceLimit, "License device limit reached.");
            }

            if (!string.IsNullOrWhiteSpace(license.AllowedNextJkt))
            {
                if (!string.Equals(license.AllowedNextJkt, deviceJkt, StringComparison.OrdinalIgnoreCase))
                {
                    throw new DomainRuleException(ErrorCodes.DeviceLimit, "Device not allowed to activate this license.");
                }
            }
            else
            {
                var historyBindings = await dbAdapter.ListBindingsByLicenseIdAsync(license.SchoolBotLicenseId, ct);
                var wasDetached = historyBindings.Any(b =>
                    string.Equals(b.DeviceJkt, deviceJkt, StringComparison.OrdinalIgnoreCase) && b.DetachedAt.HasValue);
                if (wasDetached)
                {
                    throw new DomainRuleException(ErrorCodes.DeviceLimit, "Detached device cannot re-activate without administrator approval.");
                }
            }

            var publicJwkJson = JsonSerializer.Serialize(request.DeviceJwk, JsonOptions);
            EntitlementResult? preparedActivation = null;
            await dbAdapter.CommitLicenseActivationAsync(new CommitLicenseActivationRequest(
                LicenseId: license.SchoolBotLicenseId,
                ExpectedCurrentBindingId: license.CurrentBindingId,
                ExpectedBindingEpoch: license.BindingEpoch,
                DeviceJkt: deviceJkt,
                PublicJwk: publicJwkJson,
                EntitlementExpiresAt: expiresAt
            ), ct, commit => preparedActivation = PrepareEntitlement(commit));

            if (managementStore is not null)
            {
                var mgmt = managementStore.FindLicenseById(license.SchoolBotLicenseId.ToString());
                if (mgmt is not null && !mgmt.Devices.Contains(deviceJkt))
                {
                    managementStore.Save(mgmt with { Devices = [.. mgmt.Devices, deviceJkt] });
                }
            }

            var result = preparedActivation ?? throw new InvalidOperationException("Entitlement was not prepared.");

            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                idempotencyStore[(deviceJkt, keyHash, idempotencyKey)] = (requestHash, result, now.AddSeconds(900));
            }

            return result;
        }
        finally
        {
            keyLock.Release();
        }
    }

    public async Task<EntitlementResult> ValidateAsync(
        ValidateLicenseRequest request,
        string rawJsonBody,
        CancellationToken ct = default)
    {
        if (request is null)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Key) || request.Key.Length > 256)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "A valid license key is required.");

        if (request.BindingId <= 0)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "A valid binding ID is required.");

        if (string.IsNullOrWhiteSpace(request.Challenge))
            throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Challenge is required.");

        if (string.IsNullOrWhiteSpace(request.Proof))
            throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Device proof is required.");

        var keyHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(request.Key)));
        var keyLock = licenseLocks.GetOrAdd(keyHash, _ => new SemaphoreSlim(1, 1));

        await keyLock.WaitAsync(ct);
        try
        {
            var license = await dbAdapter.GetLicenseByKeyHashAsync(keyHash, ct)
                ?? throw new DomainRuleException(ErrorCodes.LicenseInvalid, "License key not found.");

            var now = clock.GetUtcNow();
            if (license.RevokedAt.HasValue || license.ExpiresAt <= now)
                throw new DomainRuleException(ErrorCodes.LicenseInactive, "License expired or revoked.");

            var binding = await dbAdapter.GetBindingByIdAsync(request.BindingId, ct);
            if (binding is null || binding.LicenseId != license.SchoolBotLicenseId)
                throw new DomainRuleException(ErrorCodes.DeviceNotActivated, "Device is not activated for this license.");

            if (binding.DetachedAt.HasValue || license.CurrentBindingId != request.BindingId)
                throw new DomainRuleException(ErrorCodes.BindingRevoked, "Device binding has been detached or revoked.");

            DeviceJwk deviceJwk;
            try
            {
                deviceJwk = JsonSerializer.Deserialize<DeviceJwk>(binding.PublicJwk, JsonOptions)!;
            }
            catch
            {
                throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Stored device public key is corrupted.");
            }

            DeviceProofVerifier.VerifyServerChallenge(request.Challenge, signer, now, TryConsumeNonce, binding.DeviceJkt);

            var expectedBodyHash = DeviceProofVerifier.ComputeBodyHashWithoutProof(rawJsonBody);
            DeviceProofVerifier.VerifyDeviceProof(
                request.Proof,
                deviceJwk,
                request.Challenge,
                "POST",
                "/api/service-school-bot/licenses/validate",
                null,
                expectedBodyHash);

            var expiresAt = MinTime(now.AddSeconds(259200), license.ExpiresAt);

            var updatedBinding = binding with
            {
                LastCheckedAt = now,
                LastEntitlementExpiresAt = expiresAt,
                UpdateDate = now
            };
            EntitlementResult? prepared = null;
            await dbAdapter.UpdateBindingAsync(updatedBinding, ct,
                commit => prepared = PrepareEntitlement(commit));
            return prepared ?? throw new InvalidOperationException("Entitlement was not prepared.");
        }
        finally
        {
            keyLock.Release();
        }
    }

    private EntitlementResult PrepareEntitlement(LicenseActivationCommitResult commit)
    {
        var binding = commit.Binding;
        var token = IssueEntitlementToken(commit.License, binding, binding.DeviceJkt,
            binding.LastCheckedAt, binding.LastEntitlementExpiresAt);
        return new EntitlementResult(true, commit.License.SchoolBotLicenseId,
            binding.SchoolBotLicenseBindingId, token, binding.LastCheckedAt.ToUniversalTime(),
            binding.LastEntitlementExpiresAt.ToUniversalTime());
    }

    private string IssueEntitlementToken(
        SchoolBotLicenseRecord license,
        SchoolBotLicenseBindingRecord binding,
        string deviceJkt,
        DateTimeOffset checkedAt,
        DateTimeOffset expiresAt)
    {
        var checkedSec = checkedAt.ToUnixTimeSeconds();
        var expSec = expiresAt.ToUnixTimeSeconds();
        var licExpSec = license.ExpiresAt.ToUnixTimeSeconds();

        var claims = new
        {
            iss = "schoolbot-api",
            aud = "schoolbot-desktop",
            jti = Guid.NewGuid().ToString("D"),
            license_id = license.SchoolBotLicenseId,
            school_id = license.SchoolId,
            binding_id = binding.SchoolBotLicenseBindingId,
            binding_epoch = binding.Epoch,
            cnf = new { jkt = deviceJkt },
            modes = new[] { "block", "arm", "code" },
            iat = checkedSec,
            nbf = checkedSec,
            checked_at = checkedSec,
            exp = expSec,
            license_expires_at = licExpSec
        };

        return signer.Sign("schoolbot-entitlement+jwt", claims);
    }

    private static DateTimeOffset MinTime(DateTimeOffset a, DateTimeOffset b) => a < b ? a : b;
}
