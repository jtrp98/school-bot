using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class ThardwareScanLog
{
    public int LogId { get; set; }

    public DateTime ReceivedAt { get; set; }

    public int? SchoolId { get; set; }

    public string? SchoolName { get; set; }

    public int? EntityId { get; set; }

    public string? EntityType { get; set; }

    public string? EntityName { get; set; }

    public string? ScanDirection { get; set; }

    public string ScanChannel { get; set; } = null!;

    public int? DeviceType { get; set; }

    public DateTime? ClientScanTime { get; set; }

    public string? ClientScanTimeRaw { get; set; }

    public string Path { get; set; } = null!;

    public string HttpMethod { get; set; } = null!;

    public string? SourceIp { get; set; }

    public string? RequestBody { get; set; }

    public string? ParseNote { get; set; }

    public string? DeviceIdentifier { get; set; }

    public string? RequestHost { get; set; }

    public DateTime? EnrichedAt { get; set; }

    public string? CardNumber { get; set; }

    public string? CorrelationId { get; set; }

    public string? ResponseBody { get; set; }
}
