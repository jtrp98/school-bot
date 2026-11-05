using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed partial class LicenseRepository
{
    private static SchoolBotLicenseRecord From(SchoolBotLicense row) => new(
        row.SchoolBotLicenseId,
        row.SchoolName,
        row.KeyHash,
        row.KeyHint,
        Offset(row.ExpiresAt),
        Offset(row.RevokedAt),
        row.CurrentBindingId,
        row.BindingEpoch,
        row.AllowedNextJkt,
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotLicense To(SchoolBotLicenseRecord row) => new()
    {
        SchoolBotLicenseId = row.SchoolBotLicenseId,
        SchoolName = row.SchoolName,
        KeyHash = row.KeyHash,
        KeyHint = row.KeyHint,
        ExpiresAt = row.ExpiresAt.UtcDateTime,
        RevokedAt = row.RevokedAt?.UtcDateTime,
        CurrentBindingId = row.CurrentBindingId,
        BindingEpoch = row.BindingEpoch,
        AllowedNextJkt = row.AllowedNextJkt,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static SchoolBotLicenseBindingRecord From(SchoolBotLicenseBinding row) => new(
        row.SchoolBotLicenseBindingId,
        row.LicenseId,
        row.DeviceJkt,
        row.PublicJwk,
        row.Epoch,
        Offset(row.ActivatedAt),
        Offset(row.DetachedAt),
        Offset(row.LastCheckedAt),
        Offset(row.LastEntitlementExpiresAt),
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotLicenseBinding To(SchoolBotLicenseBindingRecord row) => new()
    {
        SchoolBotLicenseBindingId = row.SchoolBotLicenseBindingId,
        LicenseId = row.LicenseId,
        DeviceJkt = row.DeviceJkt,
        PublicJwk = row.PublicJwk,
        Epoch = row.Epoch,
        ActivatedAt = row.ActivatedAt.UtcDateTime,
        DetachedAt = row.DetachedAt?.UtcDateTime,
        LastCheckedAt = row.LastCheckedAt.UtcDateTime,
        LastEntitlementExpiresAt = row.LastEntitlementExpiresAt.UtcDateTime,
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
