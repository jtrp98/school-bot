using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain.Identity;
using ServiceSchoolBot.Infrastructure.Common.Databases;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed class SchoolProfileRepository(JabjaiSchoolSingleDBPostgreSQLDBContext db) : ISchoolProfileRepository
{
    public Task<LegacyStudentProfileRecord?> FindStudentProfileAsync(int schoolId, int studentId, CancellationToken ct = default) =>
        db.Tusers.AsNoTracking()
            .Where(r => r.SchoolId == schoolId && r.SId == studentId)
            .Select(r => new LegacyStudentProfileRecord(r.SId, r.SchoolId, (r.SName ?? "") + " " + (r.SLastname ?? ""),
                r.CDel == null || r.CDel == '0'))
            .SingleOrDefaultAsync(ct);

    public Task<LegacyEmployeeProfileRecord?> FindEmployeeProfileAsync(int schoolId, int employeeId, CancellationToken ct = default) =>
        db.Temployees.AsNoTracking()
            .Where(r => r.SchoolId == schoolId && r.SEmp == employeeId)
            .Select(r => new LegacyEmployeeProfileRecord(r.SEmp, r.SchoolId, (r.SName ?? "") + " " + (r.SLastname ?? ""),
                r.CDel == null || r.CDel == '0', false))
            .SingleOrDefaultAsync(ct);

    public Task<bool> IsStudentActiveAsync(int schoolId, int studentId, CancellationToken ct = default) =>
        db.Tusers.AsNoTracking().AnyAsync(r => r.SchoolId == schoolId && r.SId == studentId && (r.CDel == null || r.CDel == '0'), ct);

    public Task<bool> IsEmployeeActiveAsync(int schoolId, int employeeId, CancellationToken ct = default) =>
        db.Temployees.AsNoTracking().AnyAsync(r => r.SchoolId == schoolId && r.SEmp == employeeId && (r.CDel == null || r.CDel == '0'), ct);
}
