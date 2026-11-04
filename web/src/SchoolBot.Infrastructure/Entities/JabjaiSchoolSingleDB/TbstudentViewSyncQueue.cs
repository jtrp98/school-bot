using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TbstudentViewSyncQueue
{
    public int QueueId { get; set; }

    public int? SId { get; set; }

    public int? SchoolId { get; set; }

    public bool? Processed { get; set; }

    public DateTime? CreatedAt { get; set; }
}
