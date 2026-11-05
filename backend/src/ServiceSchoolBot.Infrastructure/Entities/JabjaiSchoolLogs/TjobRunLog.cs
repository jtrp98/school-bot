using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TjobRunLog
{
    public Guid JobRunId { get; set; }

    public string JobKey { get; set; } = null!;

    public string TriggerType { get; set; } = null!;

    public DateOnly? BusinessDate { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public long? DurationMs { get; set; }

    public string Status { get; set; } = null!;

    public int? SchoolsExpected { get; set; }

    public int? SchoolsProcessed { get; set; }

    public int? SchoolsFailed { get; set; }

    public int? RowsAffected { get; set; }

    public string? MachineName { get; set; }

    public int? ProcessId { get; set; }

    public string? CronUsed { get; set; }

    public string? TriggeredBy { get; set; }

    public string? ErrorSummary { get; set; }
}
