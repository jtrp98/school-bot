using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TattendanceConfigChangeLog
{
    public long Id { get; set; }

    public int SchoolId { get; set; }

    public string ConfigType { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? EntityId { get; set; }

    public string? EntityName { get; set; }

    public DateOnly? AffectedDateFrom { get; set; }

    public DateOnly? AffectedDateTo { get; set; }

    public string? AffectedDays { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? ChangedSummary { get; set; }

    public string? ImpactFlags { get; set; }

    public int? AffectedScanCount { get; set; }

    public string? SourcePage { get; set; }

    public string? SourceMethod { get; set; }

    public int ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? Ipaddress { get; set; }
}
