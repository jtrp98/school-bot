using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotCriterion
{
    public int SchoolBotCriteriaId { get; set; }

    public int MissionId { get; set; }

    public int Revision { get; set; }

    public string Rules { get; set; } = null!;

    public string SourceRef { get; set; } = null!;

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? SchoolId { get; set; }
}
