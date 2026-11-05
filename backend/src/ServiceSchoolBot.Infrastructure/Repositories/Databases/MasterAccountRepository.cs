using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain.Identity;
using ServiceSchoolBot.Infrastructure.Common.Databases;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed class MasterAccountRepository(JabjaiMasterSingleDBPostgreSQLDBContext db) : IMasterAccountRepository
{
    public Task<LegacyMasterAccountRecord?> FindAccountAsync(int schoolId, string username, CancellationToken ct = default)
    {
        var name = username.Trim();
        return db.TmasterDbusers.AsNoTracking()
            .Where(r => r.NCompany == schoolId && r.Username == name)
            .Select(r => new LegacyMasterAccountRecord(r.SId, schoolId, r.Username ?? "", r.CType ?? "",
                r.CDel == null || r.CDel == '0'))
            .SingleOrDefaultAsync(ct);
    }

    public Task<LegacyMasterAccountRecord?> FindAccountByIdAsync(int schoolId, int accountId, CancellationToken ct = default) =>
        db.TmasterDbusers.AsNoTracking()
            .Where(r => r.NCompany == schoolId && r.SId == accountId)
            .Select(r => new LegacyMasterAccountRecord(r.SId, schoolId, r.Username ?? "", r.CType ?? "",
                r.CDel == null || r.CDel == '0'))
            .SingleOrDefaultAsync(ct);

    public Task<bool> IsSchoolActiveAsync(int schoolId, CancellationToken ct = default) =>
        db.Tcompanies.AsNoTracking().AnyAsync(r => r.NCompany == schoolId && r.IsActive == true && r.CDel != true, ct);
}
