using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain.Projects;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed partial class ProjectRepository
{
    private static SchoolBotProjectRecord From(SchoolBotProject row) => new(
        row.SchoolBotProjectId,
        row.OwnerKind,
        row.OwnerId,
        row.Mode,
        row.Name,
        row.CurrentRevision,
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotProject To(SchoolBotProjectRecord row) => new()
    {
        SchoolBotProjectId = row.SchoolBotProjectId,
        OwnerKind = row.OwnerKind,
        OwnerId = row.OwnerId,
        Mode = row.Mode,
        Name = row.Name,
        CurrentRevision = row.CurrentRevision,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static SchoolBotProjectRevisionRecord From(SchoolBotProjectRevision row) => new(
        row.SchoolBotProjectRevisionId,
        row.ProjectId,
        row.Revision,
        row.Content,
        row.ContentHash,
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotProjectRevision To(SchoolBotProjectRevisionRecord row) => new()
    {
        SchoolBotProjectRevisionId = row.SchoolBotProjectRevisionId,
        ProjectId = row.ProjectId,
        Revision = row.Revision,
        Content = row.Content,
        ContentHash = row.ContentHash,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static DateTimeOffset Offset(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    private static DateTimeOffset? Offset(DateTime? value) => value is { } date ? Offset(date) : null;
}
