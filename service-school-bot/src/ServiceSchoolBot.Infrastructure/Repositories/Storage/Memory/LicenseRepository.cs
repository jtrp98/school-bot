using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using ServiceSchoolBot.Domain.Licensing;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed class LicenseRepository : ILicenseRepository
{
    private readonly SchoolBotRepositoryState state;

    public LicenseRepository(SchoolBotUnitOfWork unitOfWork)
    {
        state = unitOfWork.State;
    }

    public async Task<SchoolBotLicenseRecord?> GetLicenseByIdAsync(int licenseId, CancellationToken ct = default)
    {
        var snapshot = await state.backingStore.LoadLicenseUnitAsync(ct);
        var licenses = snapshot.Licenses;
        return licenses.FirstOrDefault(l => l.SchoolBotLicenseId == licenseId && !l.IsDelete);
    }

    public async Task<SchoolBotLicenseRecord?> GetLicenseByKeyHashAsync(string keyHash, CancellationToken ct = default)
    {
        var snapshot = await state.backingStore.LoadLicenseUnitAsync(ct);
        var licenses = snapshot.Licenses;
        return licenses.FirstOrDefault(l =>
            string.Equals(l.KeyHash, keyHash, StringComparison.OrdinalIgnoreCase) && !l.IsDelete);
    }

    public async Task<IReadOnlyList<SchoolBotLicenseRecord>> ListLicensesAsync(
        int? schoolId = null, CancellationToken ct = default)
    {
        var snapshot = await state.backingStore.LoadLicenseUnitAsync(ct);
        var licenses = snapshot.Licenses;
        var query = licenses.Where(l => !l.IsDelete);
        if (schoolId.HasValue) query = query.Where(l => l.SchoolId == schoolId.Value);
        return query.ToArray();
    }

    public Task<SchoolBotLicenseRecord> CreateLicenseAsync(
        SchoolBotLicenseRecord license, CancellationToken ct = default)
    {
        return state.backingStore.MutateLicenseUnitAsync(snapshot =>
        {
            var mutation = LicenseAdminService.PrepareStoredCreateLicense(
                snapshot.Licenses, snapshot.Bindings, state.clock, license);
            return new LicenseStoreMutation<SchoolBotLicenseRecord>(
                new LicenseStoreSnapshot(mutation.Licenses, mutation.Bindings), mutation.Result);
        }, ct);
    }

    public Task<SchoolBotLicenseRecord> RevokeLicenseAsync(
        int licenseId, int? revokedBy, string? expectedETag = null, CancellationToken ct = default)
    {
        return state.backingStore.MutateLicenseUnitAsync(snapshot =>
        {
            var mutation = LicenseAdminService.PrepareStoredRevokeLicense(
                snapshot.Licenses, snapshot.Bindings, state.clock, licenseId, revokedBy, expectedETag);
            return new LicenseStoreMutation<SchoolBotLicenseRecord>(
                new LicenseStoreSnapshot(mutation.Licenses, mutation.Bindings), mutation.Result);
        }, ct);
    }

    public async Task<SchoolBotLicenseBindingRecord?> GetBindingByIdAsync(int bindingId, CancellationToken ct = default)
    {
        var snapshot = await state.backingStore.LoadLicenseUnitAsync(ct);
        var bindings = snapshot.Bindings;
        return bindings.FirstOrDefault(b => b.SchoolBotLicenseBindingId == bindingId && !b.IsDelete);
    }

    public async Task<IReadOnlyList<SchoolBotLicenseBindingRecord>> ListBindingsByLicenseIdAsync(
        int licenseId, CancellationToken ct = default)
    {
        var snapshot = await state.backingStore.LoadLicenseUnitAsync(ct);
        var bindings = snapshot.Bindings;
        return bindings.Where(b => b.LicenseId == licenseId && !b.IsDelete).ToArray();
    }

    public Task<SchoolBotLicenseBindingRecord> UpdateBindingAsync(
        SchoolBotLicenseBindingRecord binding, CancellationToken ct = default,
        Action<LicenseActivationCommitResult>? prepare = null)
    {
        return state.backingStore.MutateLicenseUnitAsync(snapshot =>
        {
            var mutation = LicenseDeviceService.PrepareStoredUpdateBinding(
                snapshot.Licenses, snapshot.Bindings, state.clock, binding, prepare);
            return new LicenseStoreMutation<SchoolBotLicenseBindingRecord>(
                new LicenseStoreSnapshot(mutation.Licenses, mutation.Bindings), mutation.Result);
        }, ct);
    }

    public Task<LicenseActivationCommitResult> CommitLicenseActivationAsync(
        CommitLicenseActivationRequest request, CancellationToken ct = default,
        Action<LicenseActivationCommitResult>? prepare = null)
    {
        return state.backingStore.MutateLicenseUnitAsync(snapshot =>
        {
            var mutation = LicenseDeviceService.PrepareStoredCommitLicenseActivation(
                snapshot.Licenses, snapshot.Bindings, state.clock, request, prepare);
            return new LicenseStoreMutation<LicenseActivationCommitResult>(
                new LicenseStoreSnapshot(mutation.Licenses, mutation.Bindings), mutation.Result);
        }, ct);
    }

    public Task<SchoolBotLicenseRecord> CommitLicenseUnbindAsync(
        CommitLicenseUnbindRequest request, CancellationToken ct = default)
    {
        return state.backingStore.MutateLicenseUnitAsync(snapshot =>
        {
            var mutation = LicenseAdminService.PrepareStoredCommitLicenseUnbind(
                snapshot.Licenses, snapshot.Bindings, state.clock, request);
            return new LicenseStoreMutation<SchoolBotLicenseRecord>(
                new LicenseStoreSnapshot(mutation.Licenses, mutation.Bindings), mutation.Result);
        }, ct);
    }

}
