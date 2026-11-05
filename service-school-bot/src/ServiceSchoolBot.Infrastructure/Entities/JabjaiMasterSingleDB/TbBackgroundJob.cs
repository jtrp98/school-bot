using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbBackgroundJob
{
    public long Id { get; set; }

    public Guid JobId { get; set; }

    public int? SchoolId { get; set; }

    public int? TeacherId { get; set; }

    public string JobType { get; set; } = null!;

    public short JobStatus { get; set; }

    public short Priority { get; set; }

    public string? JobPayload { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public double? ResponseTimeMs { get; set; }

    public short RetryCount { get; set; }

    public short MaxRetry { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ProcessedBy { get; set; }

    public string? Note { get; set; }
}
