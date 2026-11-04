using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Infrastructure.Common.Databases;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed class GradeRepository(PGGradeDBPostgreSQLDBContext db) : IGradeRepository
{
    public async Task<bool> VerifyGradeWriterReadyAsync(int schoolId, CancellationToken ct = default)
    {
        if (schoolId <= 0) return false;
        if (!await db.Pgtgrades.AsNoTracking().AnyAsync(row => row.SchoolId == schoolId && !row.CDel, ct)) return false;
        return await db.Database.SqlQuery<bool>($"""
            SELECT has_table_privilege(current_user, 'public."PGTGradeDetail"', 'UPDATE')
               AND has_table_privilege(current_user, 'public."PGTGradeAuditLog"', 'INSERT') AS "Value"
            """).SingleAsync(ct);
    }
}
