using System;
using System.Collections.Generic;
using System.Net;

namespace SchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class SqlqueryLog
{
    public int? Pid { get; set; }

    public string? ApplicationName { get; set; }

    public IPAddress? ClientAddr { get; set; }

    public DateTime? BackendStart { get; set; }

    public string? State { get; set; }

    public DateTime? QueryStart { get; set; }

    public TimeSpan? QueryDuration { get; set; }

    public string? QueryText { get; set; }
}
