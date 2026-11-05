using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class LogTrace
{
    /// <summary>
    /// Unique ID for end-to-end request tracking
    /// </summary>
    public Guid TraceId { get; set; }

    public string ProjectName { get; set; } = null!;

    public string RootService { get; set; } = null!;

    public string? InitialEndpoint { get; set; }

    /// <summary>
    /// SUCCESS, FAILED, PARTIAL_SUCCESS
    /// </summary>
    public string? Status { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public int? DurationMs { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AlertNotification> AlertNotifications { get; set; } = new List<AlertNotification>();

    public virtual ICollection<ApiCallLog> ApiCallLogs { get; set; } = new List<ApiCallLog>();

    public virtual ICollection<ApplicationLog> ApplicationLogs { get; set; } = new List<ApplicationLog>();

    public virtual ICollection<SchedulesLog> SchedulesLogs { get; set; } = new List<SchedulesLog>();

    public virtual ICollection<ServiceCallLog> ServiceCallLogs { get; set; } = new List<ServiceCallLog>();
}
