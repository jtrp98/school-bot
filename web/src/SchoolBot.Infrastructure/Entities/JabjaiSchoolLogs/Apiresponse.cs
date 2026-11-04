using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class Apiresponse
{
    public int LogId { get; set; }

    public int SchoolId { get; set; }

    public string? Apirequest { get; set; }

    public string? Apiresponse1 { get; set; }

    public string? MethodName { get; set; }

    public DateTime Tstamp { get; set; }

    public int? UserId { get; set; }

    public Guid? CardId { get; set; }

    public double ResponseTime { get; set; }

    public DateTime? DeviceTime { get; set; }

    public string? SourceIp { get; set; }
}
