using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Licensing;

namespace ServiceSchoolBot.Application.Services.Implementations.Licensing;

public sealed partial class LicenseDeviceService
{
    public static (
        IReadOnlyList<SchoolBotLicenseRecord> Licenses,
        IReadOnlyList<SchoolBotLicenseBindingRecord> Bindings,
        SchoolBotLicenseBindingRecord Result) PrepareStoredUpdateBinding(
        IReadOnlyList<SchoolBotLicenseRecord> storedLicenses,
        IReadOnlyList<SchoolBotLicenseBindingRecord> storedBindings,
        TimeProvider clock,
        SchoolBotLicenseBindingRecord binding,
        Action<LicenseActivationCommitResult>? prepare = null)
    {
        var licenses = storedLicenses.ToList();
        var bindings = storedBindings.ToList();
        var index = bindings.FindIndex(b =>
            b.SchoolBotLicenseBindingId == binding.SchoolBotLicenseBindingId && !b.IsDelete);
        if (index < 0)
            throw new DomainRuleException(ErrorCodes.DeviceNotActivated, "License binding not found.");

        var existingBinding = bindings[index];
        var licenseIndex = licenses.FindIndex(l =>
            l.SchoolBotLicenseId == existingBinding.LicenseId && !l.IsDelete);
        if (licenseIndex < 0)
            throw new DomainRuleException(ErrorCodes.NotFound, "License not found.");

        var existingLicense = licenses[licenseIndex];
        var now = clock.GetUtcNow();
        if (existingLicense.RevokedAt.HasValue || existingLicense.ExpiresAt <= now)
            throw new DomainRuleException(ErrorCodes.LicenseInactive, "License expired or revoked.");

        if (existingBinding.DetachedAt.HasValue || binding.DetachedAt.HasValue ||
            existingLicense.CurrentBindingId != existingBinding.SchoolBotLicenseBindingId ||
            existingLicense.BindingEpoch != existingBinding.Epoch)
            throw new DomainRuleException(ErrorCodes.BindingRevoked, "Device binding has been detached or revoked.");

        var liveBindings = bindings.Where(b =>
            b.LicenseId == existingLicense.SchoolBotLicenseId && !b.IsDelete && b.DetachedAt == null).ToList();
        if (liveBindings.Count > 1 || (liveBindings.Count == 1 &&
            liveBindings[0].SchoolBotLicenseBindingId != existingBinding.SchoolBotLicenseBindingId))
        {
            throw new DomainRuleException(ErrorCodes.DeviceLimit, "License device limit reached.");
        }

        if (binding.LicenseId != existingBinding.LicenseId || binding.SchoolId != existingBinding.SchoolId ||
            binding.Epoch != existingBinding.Epoch || binding.DeviceJkt != existingBinding.DeviceJkt ||
            binding.PublicJwk != existingBinding.PublicJwk)
            throw new DomainRuleException(ErrorCodes.BindingRevoked, "Binding identity mismatch.");

        var checkedAt = now > existingBinding.LastCheckedAt ? now : existingBinding.LastCheckedAt;
        var updated = existingBinding with
        {
            LastCheckedAt = checkedAt,
            LastEntitlementExpiresAt = LeaseExpiry(checkedAt, existingLicense.ExpiresAt),
            UpdateDate = checkedAt
        };

        prepare?.Invoke(new LicenseActivationCommitResult(existingLicense, updated));
        bindings[index] = updated;
        return (licenses.ToArray(), bindings.ToArray(), updated);
    }

    public static (
        IReadOnlyList<SchoolBotLicenseRecord> Licenses,
        IReadOnlyList<SchoolBotLicenseBindingRecord> Bindings,
        LicenseActivationCommitResult Result) PrepareStoredCommitLicenseActivation(
        IReadOnlyList<SchoolBotLicenseRecord> storedLicenses,
        IReadOnlyList<SchoolBotLicenseBindingRecord> storedBindings,
        TimeProvider clock,
        CommitLicenseActivationRequest request,
        Action<LicenseActivationCommitResult>? prepare = null)
    {
        var licenses = storedLicenses.ToList();
        var bindings = storedBindings.ToList();
        var index = licenses.FindIndex(l => l.SchoolBotLicenseId == request.LicenseId && !l.IsDelete);
        if (index < 0)
            throw new DomainRuleException(ErrorCodes.LicenseInvalid, "License key not found.");

        var existing = licenses[index];
        var now = clock.GetUtcNow();
        if (existing.RevokedAt.HasValue || existing.ExpiresAt <= now)
            throw new DomainRuleException(ErrorCodes.LicenseInactive, "License expired or revoked.");

        if (existing.CurrentBindingId != request.ExpectedCurrentBindingId ||
            existing.BindingEpoch != request.ExpectedBindingEpoch)
        {
            throw new DomainRuleException(ErrorCodes.DeviceLimit, "License device limit reached.");
        }

        var liveBindings = bindings.Where(b =>
            b.LicenseId == existing.SchoolBotLicenseId && !b.IsDelete && b.DetachedAt == null).ToList();
        if (liveBindings.Count > 0)
        {
            throw new DomainRuleException(ErrorCodes.DeviceLimit, "License device limit reached.");
        }

        if (!string.IsNullOrWhiteSpace(existing.AllowedNextJkt))
        {
            if (!string.Equals(existing.AllowedNextJkt, request.DeviceJkt, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainRuleException(ErrorCodes.DeviceLimit, "Device not allowed to activate this license.");
            }
        }
        else
        {
            var historyBindings = bindings.Where(b =>
                b.LicenseId == existing.SchoolBotLicenseId && !b.IsDelete).ToList();
            var wasDetached = historyBindings.Any(b =>
                string.Equals(b.DeviceJkt, request.DeviceJkt, StringComparison.OrdinalIgnoreCase) &&
                b.DetachedAt.HasValue);
            if (wasDetached)
            {
                throw new DomainRuleException(ErrorCodes.DeviceLimit, "Detached device cannot re-activate without administrator approval.");
            }
        }

        var newBindingId = bindings.Count > 0 ? bindings.Max(b => b.SchoolBotLicenseBindingId) + 1 : 1;
        var candidateBinding = new SchoolBotLicenseBindingRecord(
            SchoolBotLicenseBindingId: newBindingId,
            LicenseId: existing.SchoolBotLicenseId,
            DeviceJkt: request.DeviceJkt,
            PublicJwk: request.PublicJwk,
            Epoch: existing.BindingEpoch,
            ActivatedAt: now,
            DetachedAt: null,
            LastCheckedAt: now,
            LastEntitlementExpiresAt: LeaseExpiry(now, existing.ExpiresAt),
            IsDelete: false,
            CreateBy: null,
            CreateDate: now,
            UpdateBy: null,
            UpdateDate: now,
            SchoolId: existing.SchoolId);

        var updatedLicense = existing with
        {
            CurrentBindingId = newBindingId,
            AllowedNextJkt = null,
            UpdateDate = NextLicenseUpdate(now, existing.UpdateDate)
        };

        prepare?.Invoke(new LicenseActivationCommitResult(updatedLicense, candidateBinding));
        licenses[index] = updatedLicense;
        bindings.Add(candidateBinding);

        return (licenses.ToArray(), bindings.ToArray(),
            new LicenseActivationCommitResult(updatedLicense, candidateBinding));
    }

    private static DateTimeOffset LeaseExpiry(DateTimeOffset checkedAt, DateTimeOffset expiresAt) =>
        checkedAt.AddSeconds(259200) < expiresAt ? checkedAt.AddSeconds(259200) : expiresAt;

    // Successive writes can share a clock tick; their ETags must still differ.
    private static DateTimeOffset NextLicenseUpdate(DateTimeOffset now, DateTimeOffset? previous) =>
        previous.HasValue && now <= previous.Value ? previous.Value.AddTicks(1) : now;
}
