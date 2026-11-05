using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TemailQueue
{
    public int EmailQueueId { get; set; }

    public string? Email { get; set; }

    public string? EmailSubject { get; set; }

    public string? EmailBody { get; set; }

    public DateTime? ProcessDate { get; set; }

    public string? Status { get; set; }

    public int? Retry { get; set; }
}
