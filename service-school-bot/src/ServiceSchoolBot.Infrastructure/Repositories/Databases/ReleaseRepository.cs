using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Releases;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed class ReleaseRepository(SchoolBotDbContext db, SchoolBotUnitOfWork unitOfWork) : IReleaseRepository
{
    public async Task<IReadOnlyList<DesktopRelease>> ListAsync(CancellationToken ct = default) =>
        (await db.SchoolBotReleases.AsNoTracking().Where(r => !r.IsDelete && r.WithdrawnAt == null && r.SchoolId == null)
            .Select(r => new { r.Manifest, r.Lifecycle }).ToArrayAsync(ct)).Select(r =>
                JsonSerializer.Deserialize<DesktopRelease>(r.Manifest)! with { Published = r.Lifecycle == "published" }).ToArray();

    public Task<DesktopRelease> CreateAsync(DesktopRelease release, CancellationToken ct = default) => unitOfWork.WriteAsync(async () =>
    {
        if (await db.SchoolBotReleases.AnyAsync(r => !r.IsDelete && r.VersionText == release.Version &&
            r.Channel == release.Channel && r.Platform == release.Platform, ct))
            throw new DomainRuleException(ErrorCodes.ReleaseExists, "Release identity already exists.");
        var now = unitOfWork.Now();
        db.SchoolBotReleases.Add(new SchoolBotRelease
        {
            VersionText = release.Version, Channel = release.Channel, Platform = release.Platform,
            Manifest = JsonSerializer.Serialize(release), ArtifactHash = release.Sha256, Lifecycle = "draft",
            CreateDate = now.UtcDateTime, UpdateDate = now.UtcDateTime
        });
        await db.SaveChangesAsync(ct);
        return release;
    }, ct);

    public Task<DesktopRelease> PublishAsync(string id, CancellationToken ct = default) => unitOfWork.WriteAsync(async () =>
    {
        var rows = await db.SchoolBotReleases.Where(r => !r.IsDelete && r.SchoolId == null && r.WithdrawnAt == null).ToArrayAsync(ct);
        var row = rows.SingleOrDefault(r => JsonSerializer.Deserialize<DesktopRelease>(r.Manifest)!.Id == id)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Release not found.");
        row.Lifecycle = "published";
        row.PublishedAt ??= unitOfWork.Now().UtcDateTime;
        row.UpdateDate = unitOfWork.Now().UtcDateTime;
        await db.SaveChangesAsync(ct);
        return JsonSerializer.Deserialize<DesktopRelease>(row.Manifest)! with { Published = true };
    }, ct);
}
