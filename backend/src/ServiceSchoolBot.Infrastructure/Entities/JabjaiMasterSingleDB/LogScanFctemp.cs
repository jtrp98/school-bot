using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class LogScanFctemp
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public string LogData { get; set; } = null!;

    public DateTime Tstamp { get; set; }

    public string? LogStatus { get; set; }

    public string? MethodName { get; set; }

    public string? ErrorLog { get; set; }
}
