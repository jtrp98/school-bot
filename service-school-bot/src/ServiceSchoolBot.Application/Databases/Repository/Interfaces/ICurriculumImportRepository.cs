using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface ICurriculumImportRepository
{
    Task<CurriculumImportResponse> ImportCurriculumLessonsAsync(
        int curriculumId,
        IReadOnlyList<PreparedCurriculumImportRow> rows,
        SchoolBotAuditEventRecord audit,
        CancellationToken ct = default);
}
