using ServiceSchoolBot.Domain.Identity;

namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface ISchoolProfileRepository
{
    Task<LegacyStudentProfileRecord?> FindStudentProfileAsync(int schoolId, int studentId, CancellationToken ct = default);
    Task<LegacyEmployeeProfileRecord?> FindEmployeeProfileAsync(int schoolId, int employeeId, CancellationToken ct = default);
    Task<bool> IsStudentActiveAsync(int schoolId, int studentId, CancellationToken ct = default);
    Task<bool> IsEmployeeActiveAsync(int schoolId, int employeeId, CancellationToken ct = default);
}
