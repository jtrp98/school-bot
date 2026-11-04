using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TapirequestResponse
{
    public int Id { get; set; }

    public string? MethodName { get; set; }

    public int? ExamId { get; set; }

    public int? SchoolId { get; set; }

    public int? StudentId { get; set; }

    public string? RequestData { get; set; }

    public string? ResponseData { get; set; }

    public DateTime? Tstamp { get; set; }

    public string? ServerIp { get; set; }

    public bool? Status { get; set; }
}
