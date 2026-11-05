using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotScorePreviewItem
{
    public int SchoolBotScorePreviewItemId { get; set; }

    public int ScorePreviewId { get; set; }

    public int LearnerId { get; set; }

    public string State { get; set; } = null!;

    public string ReasonCodes { get; set; } = null!;

    public string Snapshot { get; set; } = null!;

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
