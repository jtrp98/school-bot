using ServiceSchoolBot.Domain.Identity;

namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface IMasterAccountRepository
{
    Task<LegacyMasterAccountRecord?> FindAccountAsync(int schoolId, string username, CancellationToken ct = default);
    Task<LegacyMasterAccountRecord?> FindAccountByIdAsync(int schoolId, int accountId, CancellationToken ct = default);
    Task<bool> IsSchoolActiveAsync(int schoolId, CancellationToken ct = default);
}
