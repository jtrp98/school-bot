using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed partial class CurriculumImportRepository(SchoolBotDbContext db, SchoolBotUnitOfWork unitOfWork) : ICurriculumImportRepository
{
    public Task<CurriculumImportResponse> ImportCurriculumLessonsAsync(
        int curriculumId, IReadOnlyList<PreparedCurriculumImportRow> rows, SchoolBotAuditEventRecord audit,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        unitOfWork.RequireIdempotentCommit();
        // The deployed database has no ImportSource column; never discard import provenance silently.
        var table = db.Model.FindEntityType(typeof(SchoolBotLesson))!.GetTableName();
        throw new DomainRuleException(ErrorCodes.StateConflict, $"Curriculum import requires {table}.ImportSource and a refreshed scaffold.");
    }
}
