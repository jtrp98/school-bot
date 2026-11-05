using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TjobConfig
{
    public string JobKey { get; set; } = null!;

    public bool? IsEnabled { get; set; }

    public string? CronExpression { get; set; }

    public int? RolesMask { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public string InstanceKey { get; set; } = null!;
}
