using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbJobsQueue
{
    public Guid JobId { get; set; }

    public int MessageId { get; set; }

    public string? JobName { get; set; }

    public string? JobValues { get; set; }

    public string? StateName { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? FetchedDate { get; set; }

    public DateTime? SchedulerDate { get; set; }

    public DateTime? CompleteDate { get; set; }

    public string? ErrorMessage { get; set; }

    public double? ResponseTime { get; set; }
}
