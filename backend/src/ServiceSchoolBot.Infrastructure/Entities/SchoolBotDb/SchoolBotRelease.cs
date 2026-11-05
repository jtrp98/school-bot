using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotRelease
{
    public int SchoolBotReleaseId { get; set; }

    public string VersionText { get; set; } = null!;

    public string Channel { get; set; } = null!;

    public string Platform { get; set; } = null!;

    public string Manifest { get; set; } = null!;

    public string ArtifactHash { get; set; } = null!;

    public string Lifecycle { get; set; } = null!;

    public DateTime? PublishedAt { get; set; }

    public DateTime? WithdrawnAt { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? SchoolId { get; set; }
}
