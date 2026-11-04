using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class DeviceActivityLog
{
    public Guid LogId { get; set; }

    public int SchoolId { get; set; }

    public string DeviceId { get; set; } = null!;

    public string LogDescription { get; set; } = null!;

    public DateTime Tstamp { get; set; }

    public string DeviceType { get; set; } = null!;
}
