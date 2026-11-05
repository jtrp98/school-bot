using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotScorePreview
{
    public int SchoolBotScorePreviewId { get; set; }

    public int CourseRunId { get; set; }

    public string Principal { get; set; } = null!;

    public string SnapshotHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public string State { get; set; } = null!;

    public int? SupersedesId { get; set; }

    public string Context { get; set; } = null!;

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
