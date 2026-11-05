using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using System.Collections.Concurrent;
using ServiceSchoolBot.Domain.Identity;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed class SchoolProfileRepository : ISchoolProfileRepository
{
    private readonly ConcurrentDictionary<(int SchoolId, int StudentId), LegacyStudentProfileRecord> students = new();
    private readonly ConcurrentDictionary<(int SchoolId, int EmployeeId), LegacyEmployeeProfileRecord> employees = new();

    public void SeedStudent(LegacyStudentProfileRecord student)
    {
        students[(student.SchoolId, student.StudentId)] = student;
    }

    public void SeedEmployee(LegacyEmployeeProfileRecord employee)
    {
        employees[(employee.SchoolId, employee.EmployeeId)] = employee;
    }

    public Task<LegacyStudentProfileRecord?> FindStudentProfileAsync(int schoolId, int studentId, CancellationToken ct = default)
    {
        students.TryGetValue((schoolId, studentId), out var student);
        return Task.FromResult(student);
    }

    public Task<LegacyEmployeeProfileRecord?> FindEmployeeProfileAsync(int schoolId, int employeeId, CancellationToken ct = default)
    {
        employees.TryGetValue((schoolId, employeeId), out var employee);
        return Task.FromResult(employee);
    }

    public Task<bool> IsStudentActiveAsync(int schoolId, int studentId, CancellationToken ct = default)
    {
        var found = students.TryGetValue((schoolId, studentId), out var student);
        return Task.FromResult(found && student is not null && student.IsActive);
    }

    public Task<bool> IsEmployeeActiveAsync(int schoolId, int employeeId, CancellationToken ct = default)
    {
        var found = employees.TryGetValue((schoolId, employeeId), out var employee);
        return Task.FromResult(found && employee is not null && employee.IsActive);
    }
}
