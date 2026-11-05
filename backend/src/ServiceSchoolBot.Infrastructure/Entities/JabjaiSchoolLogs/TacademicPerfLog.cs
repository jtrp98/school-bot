using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TacademicPerfLog
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ServerName { get; set; }

    public string? MethodName { get; set; }

    public int? TotalMs { get; set; }

    public int? SchoolId { get; set; }

    public string? Context { get; set; }

    public string? Steps { get; set; }
}
