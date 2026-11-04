using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class WatchdogLog
{
    public int Id { get; set; }

    public string? Eventid { get; set; }

    public string? Message { get; set; }

    public DateTime Timestamp { get; set; }

    public string? Callingfrom { get; set; }

    public string? Callingmethod { get; set; }

    public int? Linenumber { get; set; }

    public string? Loglevel { get; set; }
}
