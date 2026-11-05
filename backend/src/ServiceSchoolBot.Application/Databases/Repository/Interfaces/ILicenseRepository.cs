using ServiceSchoolBot.Domain.Licensing;

namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface ILicenseRepository
{
    Task<SchoolBotLicenseRecord?> GetLicenseByIdAsync(int licenseId, CancellationToken ct = default);
    Task<SchoolBotLicenseRecord?> GetLicenseByKeyHashAsync(string keyHash, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotLicenseRecord>> ListLicensesAsync(int? schoolId = null, CancellationToken ct = default);
    Task<SchoolBotLicenseRecord> CreateLicenseAsync(SchoolBotLicenseRecord license, CancellationToken ct = default);
    Task<SchoolBotLicenseRecord> RevokeLicenseAsync(int licenseId, int? revokedBy, string? expectedETag = null, CancellationToken ct = default);
    Task<SchoolBotLicenseBindingRecord?> GetBindingByIdAsync(int bindingId, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotLicenseBindingRecord>> ListBindingsByLicenseIdAsync(int licenseId, CancellationToken ct = default);
    Task<SchoolBotLicenseBindingRecord> UpdateBindingAsync(
        SchoolBotLicenseBindingRecord binding, CancellationToken ct = default,
        Action<LicenseActivationCommitResult>? prepare = null);
    Task<LicenseActivationCommitResult> CommitLicenseActivationAsync(
        CommitLicenseActivationRequest request, CancellationToken ct = default,
        Action<LicenseActivationCommitResult>? prepare = null);
    Task<SchoolBotLicenseRecord> CommitLicenseUnbindAsync(CommitLicenseUnbindRequest request, CancellationToken ct = default);
}
