using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed partial class LicenseRepository(SchoolBotDbContext db, SchoolBotUnitOfWork unitOfWork) : ILicenseRepository
{
    public async Task<SchoolBotLicenseRecord?> GetLicenseByIdAsync(
        int licenseId, CancellationToken ct = default)
    {
        var row = await db.SchoolBotLicenses.AsNoTracking()
            .Where(r => r.SchoolBotLicenseId == licenseId && !r.IsDelete)
            .SingleOrDefaultAsync(ct);
        return row is null ? null : From(row);
    }

    public async Task<SchoolBotLicenseRecord?> GetLicenseByKeyHashAsync(
        string keyHash, CancellationToken ct = default)
    {
        var row = await db.SchoolBotLicenses.AsNoTracking()
            .Where(r => r.KeyHash.ToLower() == keyHash.ToLower() && !r.IsDelete)
            .SingleOrDefaultAsync(ct);
        return row is null ? null : From(row);
    }

    public async Task<IReadOnlyList<SchoolBotLicenseRecord>> ListLicensesAsync(
        int? schoolId = null, CancellationToken ct = default)
    {
        var rows = await db.SchoolBotLicenses.AsNoTracking()
            .Where(r => !r.IsDelete && (schoolId == null || r.SchoolId == schoolId))
            .ToArrayAsync(ct);
        return rows.Select(From).ToArray();
    }

    public async Task<SchoolBotLicenseBindingRecord?> GetBindingByIdAsync(
        int bindingId, CancellationToken ct = default)
    {
        var row = await db.SchoolBotLicenseBindings.AsNoTracking()
            .Where(r => r.SchoolBotLicenseBindingId == bindingId && !r.IsDelete)
            .SingleOrDefaultAsync(ct);
        return row is null ? null : From(row);
    }

    public async Task<IReadOnlyList<SchoolBotLicenseBindingRecord>> ListBindingsByLicenseIdAsync(
        int licenseId, CancellationToken ct = default)
    {
        var rows = await db.SchoolBotLicenseBindings.AsNoTracking()
            .Where(r => r.LicenseId == licenseId && !r.IsDelete)
            .ToArrayAsync(ct);
        return rows.Select(From).ToArray();
    }

    public Task<SchoolBotLicenseRecord> CreateLicenseAsync(SchoolBotLicenseRecord license, CancellationToken ct = default) =>
        unitOfWork.WriteAsync(async () =>
        {
            if (license.SchoolId <= 0 || string.IsNullOrWhiteSpace(license.KeyHash))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "Invalid license.");
            if (await db.SchoolBotLicenses.AnyAsync(r => r.KeyHash == license.KeyHash && !r.IsDelete, ct))
                throw new DomainRuleException(ErrorCodes.StateConflict, "License key already exists.");
            var now = unitOfWork.Now();
            var row = To(license with
            {
                SchoolBotLicenseId = 0, CreateDate = license.CreateDate ?? now, UpdateDate = now, IsDelete = false
            });
            db.SchoolBotLicenses.Add(row);
            await db.SaveChangesAsync(ct);
            return From(row);
        }, ct);

    public Task<SchoolBotLicenseRecord> RevokeLicenseAsync(
        int licenseId, int? revokedBy, string? expectedETag = null, CancellationToken ct = default) =>
        unitOfWork.WriteAsync(async () =>
        {
            var (license, bindings) = await LoadLicenseAsync(licenseId, ct);
            var mutation = LicenseAdminService.PrepareStoredRevokeLicense([license], bindings,
                new MutationClock(unitOfWork.Now(license.UpdateDate)), licenseId, revokedBy, expectedETag);
            var saved = await SaveLicenseAsync(mutation.Result, mutation.Bindings, ct);
            return From(saved.License);
        }, ct);

    public Task<SchoolBotLicenseRecord> CommitLicenseUnbindAsync(CommitLicenseUnbindRequest request, CancellationToken ct = default) =>
        unitOfWork.WriteAsync(async () =>
        {
            var (license, bindings) = await LoadLicenseAsync(request.LicenseId, ct);
            var mutation = LicenseAdminService.PrepareStoredCommitLicenseUnbind([license], bindings,
                new MutationClock(unitOfWork.Now(license.UpdateDate)), request);
            var saved = await SaveLicenseAsync(mutation.Result, mutation.Bindings, ct);
            return From(saved.License);
        }, ct);

    public Task<LicenseActivationCommitResult> CommitLicenseActivationAsync(
        CommitLicenseActivationRequest request, CancellationToken ct = default,
        Action<LicenseActivationCommitResult>? prepare = null) =>
        unitOfWork.WriteAsync(async () =>
        {
            var (license, bindings) = await LoadLicenseAsync(request.LicenseId, ct);
            var mutation = LicenseDeviceService.PrepareStoredCommitLicenseActivation([license], bindings,
                new MutationClock(unitOfWork.Now(license.UpdateDate)), request);
            var saved = await SaveLicenseAsync(mutation.Result.License, mutation.Bindings, ct);
            var binding = saved.Bindings.Single(r => r.SchoolBotLicenseBindingId == saved.License.CurrentBindingId);
            var result = new LicenseActivationCommitResult(From(saved.License), From(binding));
            prepare?.Invoke(result);
            return result;
        }, ct);

    public Task<SchoolBotLicenseBindingRecord> UpdateBindingAsync(
        SchoolBotLicenseBindingRecord binding, CancellationToken ct = default,
        Action<LicenseActivationCommitResult>? prepare = null) =>
        unitOfWork.WriteAsync(async () =>
        {
            var (license, bindings) = await LoadLicenseAsync(binding.LicenseId, ct);
            var mutation = LicenseDeviceService.PrepareStoredUpdateBinding([license], bindings,
                new MutationClock(unitOfWork.Now()), binding);
            var saved = await SaveLicenseAsync(license, mutation.Bindings, ct);
            var result = From(saved.Bindings.Single(r => r.SchoolBotLicenseBindingId == binding.SchoolBotLicenseBindingId));
            prepare?.Invoke(new(From(saved.License), result));
            return result;
        }, ct);

    private async Task<(SchoolBotLicenseRecord License, SchoolBotLicenseBindingRecord[] Bindings)> LoadLicenseAsync(int id, CancellationToken ct)
    {
        var license = await db.SchoolBotLicenses.SingleOrDefaultAsync(r => r.SchoolBotLicenseId == id && !r.IsDelete, ct)
            ?? throw SchoolBotUnitOfWork.Missing("License");
        var bindings = await db.SchoolBotLicenseBindings.Where(r => r.LicenseId == id && r.SchoolId == license.SchoolId)
            .ToArrayAsync(ct);
        return (From(license), bindings.Select(From).ToArray());
    }

    private async Task<(SchoolBotLicense License, SchoolBotLicenseBinding[] Bindings)> SaveLicenseAsync(
        SchoolBotLicenseRecord license, IReadOnlyList<SchoolBotLicenseBindingRecord> bindings, CancellationToken ct)
    {
        var row = db.SchoolBotLicenses.Local.Single(r => r.SchoolBotLicenseId == license.SchoolBotLicenseId);
        var saved = new List<SchoolBotLicenseBinding>();
        SchoolBotLicenseBinding? newCurrent = null;
        foreach (var binding in bindings)
        {
            var existing = db.SchoolBotLicenseBindings.Local.SingleOrDefault(r => r.LicenseId == license.SchoolBotLicenseId && r.SchoolBotLicenseBindingId == binding.SchoolBotLicenseBindingId);
            if (existing is null)
            {
                existing = To(binding with { SchoolBotLicenseBindingId = 0 });
                db.SchoolBotLicenseBindings.Add(existing);
                if (license.CurrentBindingId == binding.SchoolBotLicenseBindingId) newCurrent = existing;
            }
            else db.Entry(existing).CurrentValues.SetValues(To(binding));
            saved.Add(existing);
        }
        await db.SaveChangesAsync(ct);
        db.Entry(row).CurrentValues.SetValues(To(license with
        {
            CurrentBindingId = newCurrent?.SchoolBotLicenseBindingId ?? license.CurrentBindingId
        }));
        await db.SaveChangesAsync(ct);
        return (row, saved.ToArray());
    }

    private sealed class MutationClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
