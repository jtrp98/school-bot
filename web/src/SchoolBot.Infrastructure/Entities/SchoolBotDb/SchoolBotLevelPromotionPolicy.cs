using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotLevelPromotionPolicy
{
    public int SchoolBotLevelPromotionPolicyId { get; set; }

    public int LevelId { get; set; }

    public int Revision { get; set; }

    public decimal Threshold { get; set; }

    public string Reason { get; set; } = null!;

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? SchoolId { get; set; }
}
