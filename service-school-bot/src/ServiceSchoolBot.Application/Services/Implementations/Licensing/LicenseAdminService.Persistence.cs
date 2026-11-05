using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Licensing;

namespace ServiceSchoolBot.Application.Services.Implementations.Licensing;

public sealed partial class LicenseAdminService
{
    public static (
        IReadOnlyList<SchoolBotLicenseRecord> Licenses,
        IReadOnlyList<SchoolBotLicenseBindingRecord> Bindings,
        SchoolBotLicenseRecord Result) PrepareStoredCreateLicense(
        IReadOnlyList<SchoolBotLicenseRecord> storedLicenses,
        IReadOnlyList<SchoolBotLicenseBindingRecord> storedBindings,
        TimeProvider clock,
        SchoolBotLicenseRecord license)
    {
        var licenses = storedLicenses.ToList();
        var bindings = storedBindings.ToList();
        var now = clock.GetUtcNow();
        var id = licenses.Count > 0 ? licenses.Max(l => l.SchoolBotLicenseId) + 1 : 1;
        var created = license with
        {
            SchoolBotLicenseId = id,
            CreateDate = license.CreateDate ?? now,
            UpdateDate = now,
            IsDelete = false
        };
        licenses.Add(created);
        return (licenses.ToArray(), bindings.ToArray(), created);
    }

    public static (
        IReadOnlyList<SchoolBotLicenseRecord> Licenses,
        IReadOnlyList<SchoolBotLicenseBindingRecord> Bindings,
        SchoolBotLicenseRecord Result) PrepareStoredRevokeLicense(
        IReadOnlyList<SchoolBotLicenseRecord> storedLicenses,
        IReadOnlyList<SchoolBotLicenseBindingRecord> storedBindings,
        TimeProvider clock,
        int licenseId, int? revokedBy, string? expectedETag = null)
    {
        var licenses = storedLicenses.ToList();
        var bindings = storedBindings.ToList();
        var index = licenses.FindIndex(l => l.SchoolBotLicenseId == licenseId && !l.IsDelete);
        if (index < 0)
            throw new DomainRuleException(ErrorCodes.NotFound, "License not found.");

        var existing = licenses[index];
        if (expectedETag is not null && !OpaqueConcurrencyToken.Matches(expectedETag, existing.UpdateDate))
            throw new DomainRuleException(ErrorCodes.VersionConflict, "Concurrency token mismatch.");

        var now = clock.GetUtcNow();
        var updated = existing with
        {
            RevokedAt = now,
            UpdateBy = revokedBy,
            UpdateDate = NextLicenseUpdate(now, existing.UpdateDate)
        };
        licenses[index] = updated;
        return (licenses.ToArray(), bindings.ToArray(), updated);
    }

    public static (
        IReadOnlyList<SchoolBotLicenseRecord> Licenses,
        IReadOnlyList<SchoolBotLicenseBindingRecord> Bindings,
        SchoolBotLicenseRecord Result) PrepareStoredCommitLicenseUnbind(
        IReadOnlyList<SchoolBotLicenseRecord> storedLicenses,
        IReadOnlyList<SchoolBotLicenseBindingRecord> storedBindings,
        TimeProvider clock,
        CommitLicenseUnbindRequest request)
    {
        var licenses = storedLicenses.ToList();
        var bindings = storedBindings.ToList();
        var index = licenses.FindIndex(l => l.SchoolBotLicenseId == request.LicenseId && !l.IsDelete);
        if (index < 0)
            throw new DomainRuleException(ErrorCodes.NotFound, "License not found.");

        var existing = licenses[index];
        if (request.ExpectedETag is not null &&
            !OpaqueConcurrencyToken.Matches(request.ExpectedETag, existing.UpdateDate))
            throw new DomainRuleException(ErrorCodes.VersionConflict, "Concurrency token mismatch.");

        var now = clock.GetUtcNow();
        var candidateBindings = new List<SchoolBotLicenseBindingRecord>(bindings);
        for (var i = 0; i < candidateBindings.Count; i++)
        {
            var b = candidateBindings[i];
            if (b.LicenseId == existing.SchoolBotLicenseId && !b.IsDelete && !b.DetachedAt.HasValue)
            {
                candidateBindings[i] = b with
                {
                    DetachedAt = now,
                    UpdateBy = request.UpdatedBy,
                    UpdateDate = now
                };
            }
        }

        var updatedLicense = existing with
        {
            CurrentBindingId = null,
            BindingEpoch = existing.BindingEpoch + 1,
            AllowedNextJkt = request.NextDeviceJkt,
            UpdateBy = request.UpdatedBy,
            UpdateDate = NextLicenseUpdate(now, existing.UpdateDate)
        };

        licenses[index] = updatedLicense;
        bindings.Clear();
        bindings.AddRange(candidateBindings);

        return (licenses.ToArray(), bindings.ToArray(), updatedLicense);
    }

    // Successive writes can share a clock tick; their ETags must still differ.
    private static DateTimeOffset NextLicenseUpdate(DateTimeOffset now, DateTimeOffset? previous) =>
        previous.HasValue && now <= previous.Value ? previous.Value.AddTicks(1) : now;
}
