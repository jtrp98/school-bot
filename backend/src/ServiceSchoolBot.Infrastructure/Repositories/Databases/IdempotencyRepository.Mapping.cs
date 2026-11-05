using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed partial class IdempotencyRepository
{
    private static CurriculumIdempotencyRecord From(SchoolBotIdempotency row) => new(
        row.SchoolBotIdempotencyId,
        row.Principal,
        row.Operation,
        row.RequestKey,
        row.RequestHash,
        row.NonceHash,
        row.HttpStatus,
        row.ResultCipher,
        row.ResultKeyId,
        row.ResourceId,
        Offset(row.CompletedAt),
        Offset(row.ReplayUntil),
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotIdempotency To(CurriculumIdempotencyRecord row) => new()
    {
        SchoolBotIdempotencyId = row.SchoolBotIdempotencyId,
        Principal = row.Principal,
        Operation = row.Operation,
        RequestKey = row.RequestKey,
        RequestHash = row.RequestHash,
        NonceHash = row.NonceHash,
        HttpStatus = row.HttpStatus,
        ResultCipher = row.ResultCipher,
        ResultKeyId = row.ResultKeyId,
        ResourceId = row.ResourceId,
        CompletedAt = row.CompletedAt.UtcDateTime,
        ReplayUntil = row.ReplayUntil.UtcDateTime,
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
