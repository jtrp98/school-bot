using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotScoreExport
{
    public int SchoolBotScoreExportId { get; set; }

    public int ScorePreviewId { get; set; }

    public string ConfirmedBy { get; set; } = null!;

    public DateTime ConfirmedAt { get; set; }

    public string State { get; set; } = null!;

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
