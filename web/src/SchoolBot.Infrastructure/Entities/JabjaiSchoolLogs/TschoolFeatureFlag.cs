using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TschoolFeatureFlag
{
    public int SchoolId { get; set; }

    public string FeatureKey { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
