using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class SchedulesLog
{
    public long Id { get; set; }

    /// <summary>
    /// Link to master trace
    /// </summary>
    public Guid? TraceId { get; set; }

    public string ScheduleName { get; set; } = null!;

    public string ScheduleCode { get; set; } = null!;

    public string? ProjectName { get; set; }

    public DateTime RunAt { get; set; }

    /// <summary>
    /// 0: Pending, 1: Success, 2: Failed
    /// </summary>
    public int? Status { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual LogTrace? Trace { get; set; }
}
