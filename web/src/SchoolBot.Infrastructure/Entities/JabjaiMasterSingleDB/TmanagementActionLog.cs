using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TmanagementActionLog
{
    public long ManagementActionLogId { get; set; }

    public string Module { get; set; } = null!;

    public string ActionType { get; set; } = null!;

    public int? SchoolId { get; set; }

    public int UserId { get; set; }

    public int? AffectedCount { get; set; }

    public string? Parameters { get; set; }

    public DateTime? CreateDate { get; set; }
}
