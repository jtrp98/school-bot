using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TeventLog
{
    public long EventLogId { get; set; }

    public int? SchoolId { get; set; }

    public int? UserId { get; set; }

    public string? EventName { get; set; }

    public string? TableName { get; set; }

    public string? ColumnName { get; set; }

    public string? KeyValue { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime? CreateDate { get; set; }
}
