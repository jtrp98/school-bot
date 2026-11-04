using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class ApiLogsAcademic
{
    public int Id { get; set; }

    public DateTime? Tstamp { get; set; }

    public string? Path { get; set; }

    public string? Requestbody { get; set; }

    public string? Responsebody { get; set; }

    public string? Exceptions { get; set; }

    public string? Starttime { get; set; }

    public string? Endtime { get; set; }

    public string Responsetime { get; set; } = null!;

    public string? Sourceip { get; set; }

    public string? Requestheader { get; set; }

    public string? Applicationname { get; set; }

    public int? Schoolid { get; set; }
}
