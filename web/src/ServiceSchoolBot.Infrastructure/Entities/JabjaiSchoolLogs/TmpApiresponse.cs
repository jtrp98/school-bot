using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TmpApiresponse
{
    public int? LogId { get; set; }

    public int? SchoolId { get; set; }

    public string? Apirequest { get; set; }

    public string? Apiresponse { get; set; }

    public string? MethodName { get; set; }

    public DateTime? Tstamp { get; set; }

    public int? UserId { get; set; }

    public Guid? CardId { get; set; }

    public double? ResponseTime { get; set; }

    public DateTime? DeviceTime { get; set; }

    public string? SourceIp { get; set; }
}
