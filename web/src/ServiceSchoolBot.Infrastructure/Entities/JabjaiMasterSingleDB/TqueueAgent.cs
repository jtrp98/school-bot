using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TqueueAgent
{
    public int QueueAgentId { get; set; }

    public int StaffId { get; set; }

    public string Status { get; set; } = null!;

    public int MaxConcurrent { get; set; }

    public DateTime LastActivityDate { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual ICollection<TqueueChat> TqueueChats { get; set; } = new List<TqueueChat>();

    public virtual ICollection<TqueueLog> TqueueLogs { get; set; } = new List<TqueueLog>();
}
