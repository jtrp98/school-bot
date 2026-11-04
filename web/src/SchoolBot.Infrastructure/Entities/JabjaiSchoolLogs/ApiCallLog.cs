using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class ApiCallLog
{
    public long Id { get; set; }

    public Guid? TraceId { get; set; }

    public string? CallerService { get; set; }

    public string? TargetService { get; set; }

    /// <summary>
    /// GET, POST, etc.
    /// </summary>
    public string? Method { get; set; }

    public string? Endpoint { get; set; }

    public short? StatusCode { get; set; }

    public string? RequestBody { get; set; }

    public string? ResponseBody { get; set; }

    public int? DurationMs { get; set; }

    public bool? IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? CalledAt { get; set; }

    public virtual LogTrace? Trace { get; set; }
}
