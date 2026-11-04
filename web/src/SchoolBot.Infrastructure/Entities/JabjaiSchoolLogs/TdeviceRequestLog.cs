using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TdeviceRequestLog
{
    public long LogId { get; set; }

    public int SchoolId { get; set; }

    public string? DeviceId { get; set; }

    public string? UserCode { get; set; }

    public string LogDescription { get; set; } = null!;

    public DateTime Tstamp { get; set; }

    public string? DeviceType { get; set; }

    public string? Activity { get; set; }
}
