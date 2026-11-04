using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TexceptionLog
{
    public long ExceptionLogId { get; set; }

    public string? MethodName { get; set; }

    public string? Parameter { get; set; }

    public string? Application { get; set; }

    public string? ExceptionMsg { get; set; }

    public string? ExceptionType { get; set; }

    public string? ExceptionSource { get; set; }

    public string? ExceptionUrl { get; set; }

    public DateTime? Logdate { get; set; }

    public int? SchoolId { get; set; }

    public int? SEmp { get; set; }
}
