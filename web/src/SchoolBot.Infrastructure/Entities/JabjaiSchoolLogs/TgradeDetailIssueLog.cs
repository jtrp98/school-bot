using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TgradeDetailIssueLog
{
    public int GradeDetailIssueLogId { get; set; }

    public int NGradeId { get; set; }

    public int SchoolId { get; set; }

    public string? CourseCode { get; set; }

    public string? SubLevel { get; set; }

    public string? SubLevel2 { get; set; }

    public int? NumberYear { get; set; }

    public string? STerm { get; set; }

    public string? IssueType { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? ProcessedBy { get; set; }
}
