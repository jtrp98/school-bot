using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class SbInsightRecentActivity
{
    public int? SchoolId { get; set; }

    public DateTime? OccurredAt { get; set; }

    public string? ModuleName { get; set; }

    public string? Description { get; set; }

    public virtual SbInsightSchoolSummary? School { get; set; }
}
