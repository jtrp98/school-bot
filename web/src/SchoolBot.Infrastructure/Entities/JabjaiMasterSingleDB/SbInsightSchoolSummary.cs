using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class SbInsightSchoolSummary
{
    public int SchoolId { get; set; }

    public string SchoolName { get; set; } = null!;

    public string? Province { get; set; }

    public string? Segment { get; set; }

    public int? StudentCount { get; set; }

    public int? StaffCount { get; set; }

    public decimal? ActiveRate { get; set; }

    public decimal? RiskScore { get; set; }

    public decimal? OpportunityScore { get; set; }

    public DateOnly? LastActivityDate { get; set; }

    public List<string>? EnabledModules { get; set; }

    public virtual ICollection<SbInsightModuleUsage> SbInsightModuleUsages { get; set; } = new List<SbInsightModuleUsage>();

    public virtual ICollection<SbInsightNote> SbInsightNotes { get; set; } = new List<SbInsightNote>();
}
