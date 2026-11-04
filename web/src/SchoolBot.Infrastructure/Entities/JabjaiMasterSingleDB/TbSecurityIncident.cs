using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbSecurityIncident
{
    public long Id { get; set; }

    public string IncidentType { get; set; } = null!;

    public string Severity { get; set; } = null!;

    public int SchoolId { get; set; }

    public string? SourceIp { get; set; }

    public int? TargetUserId { get; set; }

    public string? TargetUsername { get; set; }

    public string ThreatLevel { get; set; } = null!;

    public int AttemptsCount { get; set; }

    public int? TimeWindow { get; set; }

    public int? BlockDuration { get; set; }

    public string? Description { get; set; }

    public DateTime DetectedDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool IsResolved { get; set; }

    public DateTime? ResolvedDate { get; set; }

    public string? ResolvedBy { get; set; }
}
