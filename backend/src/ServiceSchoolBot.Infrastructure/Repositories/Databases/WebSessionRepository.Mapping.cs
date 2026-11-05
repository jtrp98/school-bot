using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain.Identity;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed partial class WebSessionRepository
{
    private static SchoolBotSessionRecord From(SchoolBotSession row) => new(
        row.SchoolBotSessionId,
        row.Channel,
        row.AccountId,
        row.TokenHash,
        row.ProfileKind,
        row.ProfileId,
        row.LegacyAdminId,
        row.DeviceJkt,
        Offset(row.SourceTokenExpiresAt),
        Offset(row.ExpiresAt),
        Offset(row.RevokedAt),
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotSession To(SchoolBotSessionRecord row) => new()
    {
        SchoolBotSessionId = row.SchoolBotSessionId,
        Channel = row.Channel,
        AccountId = row.AccountId,
        TokenHash = row.TokenHash,
        ProfileKind = row.ProfileKind,
        ProfileId = row.ProfileId,
        LegacyAdminId = row.LegacyAdminId,
        DeviceJkt = row.DeviceJkt,
        SourceTokenExpiresAt = row.SourceTokenExpiresAt?.UtcDateTime,
        ExpiresAt = row.ExpiresAt.UtcDateTime,
        RevokedAt = row.RevokedAt?.UtcDateTime,
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
