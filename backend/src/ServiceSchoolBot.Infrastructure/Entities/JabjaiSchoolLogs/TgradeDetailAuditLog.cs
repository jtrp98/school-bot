using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TgradeDetailAuditLog
{
    public long GradeDetailAuditLogId { get; set; }

    public string? MethodName { get; set; }

    public string? Parameter { get; set; }

    public string? Application { get; set; }

    public DateTime? Logdate { get; set; }

    public int? SchoolId { get; set; }

    public int? SEmp { get; set; }
}
