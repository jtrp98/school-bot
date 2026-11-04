using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TstudentCallLog
{
    public int LogId { get; set; }

    public DateTime? LogDate { get; set; }

    public string? LogText { get; set; }

    public int? SchoolId { get; set; }

    public int? SId { get; set; }

    public short? Type { get; set; }

    public string? Ipaddress { get; set; }

    public string? Token { get; set; }

    public string? ConnectionId { get; set; }
}
