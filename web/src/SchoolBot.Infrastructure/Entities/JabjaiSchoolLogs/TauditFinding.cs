using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TauditFinding
{
    public long FindingId { get; set; }

    public DateOnly WeekStart { get; set; }

    public string Topic { get; set; } = null!;

    public int? SchoolId { get; set; }

    public DateOnly? BusinessDate { get; set; }

    /// <summary>
    /// Red or Yellow only — matches the dashboard dot color. Unknown/absent data is not a finding.
    /// </summary>
    public string Severity { get; set; } = null!;

    /// <summary>
    /// Open (default) -&gt; Acknowledged -&gt; Resolved | AcceptedRisk. Resolved and AcceptedRisk both count as &quot;closed&quot; for the weekly open/closed count.
    /// </summary>
    public string Status { get; set; } = null!;

    public int? AssignedTo { get; set; }

    public string? DetailJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public string? Note { get; set; }
}
