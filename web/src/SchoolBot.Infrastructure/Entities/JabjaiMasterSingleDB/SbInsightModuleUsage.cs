using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class SbInsightModuleUsage
{
    public int SchoolId { get; set; }

    public string ModuleName { get; set; } = null!;

    public bool? Enabled { get; set; }

    public int? UsageCount30Days { get; set; }

    public DateOnly? LastUsedDate { get; set; }

    public decimal? AdoptionRate { get; set; }

    public virtual SbInsightSchoolSummary School { get; set; } = null!;
}
