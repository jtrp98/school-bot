using ServiceSchoolBot.Domain.Curriculum;
namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface ISchoolBotRepositoryScope :
    IScoreExportRepository,
    IAssessmentRepository,
    ILicenseRepository,
    IProjectRepository,
    IWebSessionRepository,
    ICurriculumRepository,
    ICurriculumImportRepository
{
    new Task<IReadOnlyList<SchoolBotAuditEventRecord>> ListAuditEventsByEntityAsync(
        string entityType, int entityId, CancellationToken ct = default);
}
