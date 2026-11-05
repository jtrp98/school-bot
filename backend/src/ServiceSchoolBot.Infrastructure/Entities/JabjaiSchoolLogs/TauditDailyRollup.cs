using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TauditDailyRollup
{
    public DateOnly AuditDate { get; set; }

    public string TopicKey { get; set; } = null!;

    public string MetricKey { get; set; } = null!;

    public int? TotalCount { get; set; }

    public int ProblemCount { get; set; }

    public string? DetailJson { get; set; }

    public DateTime ComputedAt { get; set; }

    public int? ComputeDurationMs { get; set; }

    public string ComputeStatus { get; set; } = null!;

    public string? ComputeError { get; set; }
}
