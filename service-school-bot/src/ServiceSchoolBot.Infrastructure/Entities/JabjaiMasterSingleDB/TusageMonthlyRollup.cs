using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TusageMonthlyRollup
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public int FeatureId { get; set; }

    public string GroupName { get; set; } = null!;

    public string YearMonth { get; set; } = null!;

    public int RequestCount { get; set; }

    public int ActiveDays { get; set; }

    public int DistinctPaths { get; set; }

    public DateTime ComputedAt { get; set; }
}
