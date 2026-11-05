using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TqueueLog
{
    public int QueueLogId { get; set; }

    public int QueueChatId { get; set; }

    public int? QueueAgentId { get; set; }

    public string Action { get; set; } = null!;

    public string? FromStatus { get; set; }

    public string? ToStatus { get; set; }

    public string? Note { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual TqueueAgent? QueueAgent { get; set; }

    public virtual TqueueChat QueueChat { get; set; } = null!;
}
