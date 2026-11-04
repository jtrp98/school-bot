using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TempWatchdoglog
{
    public string? AuthKey { get; set; }

    public string? Token { get; set; }

    public int? SchoolId { get; set; }

    public int? UserId { get; set; }

    public DateTime? Timestamp { get; set; }
}
