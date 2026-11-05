using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TbResourceUsageLog
{
    public int ResourceUsageLogId { get; set; }

    public DateTime CapturedAt { get; set; }

    public string MachineName { get; set; } = null!;

    public decimal? ProcessCpuPercent { get; set; }

    public decimal? ProcessWorkingSetMb { get; set; }

    public decimal? ProcessPrivateMemoryMb { get; set; }

    public int? ProcessThreadCount { get; set; }

    public int? ProcessHandleCount { get; set; }

    public decimal? SystemCpuPercent { get; set; }

    public decimal? SystemAvailableMemoryMb { get; set; }

    public decimal? SystemTotalMemoryMb { get; set; }

    public string? DbConnectionCountsJson { get; set; }
}
