using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TacademicLog
{
    public int AcademicLogId { get; set; }

    public int SEmp { get; set; }

    public DateTime DLog { get; set; }

    public string? SLog { get; set; }

    public int? NSession { get; set; }

    public int? NMenu { get; set; }

    public int? Action { get; set; }

    public int? Systemtype { get; set; }

    public string? Ip { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? CDel { get; set; }

    public int? SystemLogId { get; set; }
}
