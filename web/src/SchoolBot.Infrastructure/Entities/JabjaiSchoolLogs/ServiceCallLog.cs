using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class ServiceCallLog
{
    public long Id { get; set; }

    public Guid? TraceId { get; set; }

    public string? CallerService { get; set; }

    public string? TargetService { get; set; }

    public string? GrpcService { get; set; }

    public string? GrpcMethod { get; set; }

    public string? GrpcStatus { get; set; }

    public string? RequestProto { get; set; }

    public string? ResponseProto { get; set; }

    public int? DurationMs { get; set; }

    public bool? IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? CalledAt { get; set; }

    public virtual LogTrace? Trace { get; set; }
}
