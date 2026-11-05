using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using System.Collections.Concurrent;
using ServiceSchoolBot.Domain.Identity;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed class MasterAccountRepository : IMasterAccountRepository
{
    private readonly ConcurrentDictionary<(int SchoolId, string Username), LegacyMasterAccountRecord> accounts = new();
    private readonly ConcurrentDictionary<int, bool> activeSchools = new();

    public void SeedAccount(LegacyMasterAccountRecord account)
    {
        accounts[(account.SchoolId, account.Username.Trim())] = account;
        activeSchools[account.SchoolId] = true;
    }

    public Task<LegacyMasterAccountRecord?> FindAccountAsync(int schoolId, string username, CancellationToken ct = default)
    {
        accounts.TryGetValue((schoolId, username.Trim()), out var account);
        return Task.FromResult(account);
    }

    public Task<LegacyMasterAccountRecord?> FindAccountByIdAsync(int schoolId, int accountId, CancellationToken ct = default)
    {
        var account = accounts.Values.FirstOrDefault(a => a.SchoolId == schoolId && a.AccountId == accountId);
        return Task.FromResult(account);
    }

    public Task<bool> IsSchoolActiveAsync(int schoolId, CancellationToken ct = default)
    {
        return Task.FromResult(activeSchools.GetValueOrDefault(schoolId, true));
    }
}
