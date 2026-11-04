using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class AlertNotification
{
    public long Id { get; set; }

    /// <summary>
    /// Optional: Link to trace if alert triggered by specific request
    /// </summary>
    public Guid? TraceId { get; set; }

    /// <summary>
    /// CRITICAL, FATAL, EMERGENCY
    /// </summary>
    public string? Severity { get; set; }

    public string? SourceService { get; set; }

    public string? AlertType { get; set; }

    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    public bool? IsSent { get; set; }

    public DateTime? SentAt { get; set; }

    public int? RetryCount { get; set; }

    /// <summary>
    /// PENDING, ACKNOWLEDGED, RESOLVED
    /// </summary>
    public string? Status { get; set; }

    public string? ResolvedBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Winter is coming!!!
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    public virtual LogTrace? Trace { get; set; }
}
