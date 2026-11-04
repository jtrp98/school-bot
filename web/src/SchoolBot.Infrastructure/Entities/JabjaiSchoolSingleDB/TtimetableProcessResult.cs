using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TtimetableProcessResult
{
    public long TimetableProcessResultId { get; set; }

    public string? CorrelationId { get; set; }

    public string? RequestId { get; set; }

    public string? WebhookId { get; set; }

    public string? Status { get; set; }

    public int? SchoolId { get; set; }

    public int? Nyear { get; set; }

    public string? Nterm { get; set; }

    public int? NtsubLevel { get; set; }

    public int? CurriculumId { get; set; }

    public int? PlanId { get; set; }

    public string? Payload { get; set; }

    public int? InsertedUserId { get; set; }

    public DateTime? InsertedDateTime { get; set; }

    public int? UpdatedUserId { get; set; }

    public DateTime? UpdatedDateTime { get; set; }
}
