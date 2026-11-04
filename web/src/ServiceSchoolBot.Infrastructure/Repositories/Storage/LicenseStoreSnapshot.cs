using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Licensing;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage;

public sealed record LicenseStoreSnapshot(
    IReadOnlyList<SchoolBotLicenseRecord> Licenses,
    IReadOnlyList<SchoolBotLicenseBindingRecord> Bindings)
{
    public void ValidateMutation(LicenseStoreSnapshot previous)
    {
        if (Licenses.Select(l => l.SchoolBotLicenseId).Distinct().Count() != Licenses.Count ||
            Bindings.Select(b => b.SchoolBotLicenseBindingId).Distinct().Count() != Bindings.Count)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Duplicate license or binding identity.");

        if (Bindings.Where(b => !b.IsDelete && b.DetachedAt is null)
            .GroupBy(b => b.LicenseId).Any(group => group.Count() > 1))
            throw new DomainRuleException(ErrorCodes.DeviceLimit, "License device limit reached.");

        foreach (var license in Licenses.Where(l => l.CurrentBindingId.HasValue))
        {
            if (!Bindings.Any(b => b.SchoolBotLicenseBindingId == license.CurrentBindingId &&
                b.LicenseId == license.SchoolBotLicenseId && b.SchoolId == license.SchoolId &&
                b.Epoch == license.BindingEpoch && !b.IsDelete && b.DetachedAt is null))
                throw new DomainRuleException(ErrorCodes.BindingRevoked, "Current binding is not live.");
        }

        foreach (var detached in previous.Bindings.Where(b => b.DetachedAt.HasValue))
        {
            if (!Bindings.Contains(detached))
                throw new DomainRuleException(ErrorCodes.BindingRevoked, "Detached binding history is immutable.");
        }
    }
}

public sealed record LicenseStoreMutation<T>(LicenseStoreSnapshot Snapshot, T Result);
