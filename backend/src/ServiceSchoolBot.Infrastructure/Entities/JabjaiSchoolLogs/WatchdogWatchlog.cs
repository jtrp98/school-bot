using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class WatchdogWatchlog
{
    public int Id { get; set; }

    public string? Responsebody { get; set; }

    public int Responsestatus { get; set; }

    public string? Requestbody { get; set; }

    public string? Querystring { get; set; }

    public string? Path { get; set; }

    public string? Requestheaders { get; set; }

    public string? Responseheaders { get; set; }

    public string? Method { get; set; }

    public string? Host { get; set; }

    public string? Ipaddress { get; set; }

    public string? Timespent { get; set; }

    public DateTime Starttime { get; set; }

    public DateTime Endtime { get; set; }
}
