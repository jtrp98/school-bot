using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TlineQueue
{
    public int LineQueueId { get; set; }

    public int UserId { get; set; }

    public string? LineAccesstoken { get; set; }

    public string? Message { get; set; }

    public DateTime? ProcessDate { get; set; }

    public string? Status { get; set; }

    public int? Retry { get; set; }
}
