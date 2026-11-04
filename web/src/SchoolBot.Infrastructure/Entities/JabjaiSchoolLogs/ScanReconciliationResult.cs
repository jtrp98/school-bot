using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class ScanReconciliationResult
{
    public long Id { get; set; }

    public int? SchoolId { get; set; }

    public string? DeviceId { get; set; }

    public DateTime? ScanTime { get; set; }

    public Guid? DeviceActivityLogId { get; set; }

    public int? HardwareScanLogId { get; set; }

    public string Status { get; set; } = null!;

    public int? TimeDiffSeconds { get; set; }

    public DateTime DetectedAt { get; set; }

    public string? DeviceSideNote { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public string? ResolvedBy { get; set; }

    public string? ResolutionAction { get; set; }

    public string? Note { get; set; }
}
