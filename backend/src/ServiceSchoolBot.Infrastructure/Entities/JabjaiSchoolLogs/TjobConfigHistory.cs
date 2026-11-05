using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TjobConfigHistory
{
    public long HistoryId { get; set; }

    public string JobKey { get; set; } = null!;

    public string FieldName { get; set; } = null!;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime ChangedAt { get; set; }

    public int? ChangedBy { get; set; }

    public string? InstanceKey { get; set; }
}
