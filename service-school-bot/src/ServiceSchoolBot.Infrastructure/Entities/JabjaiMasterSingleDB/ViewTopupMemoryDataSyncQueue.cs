using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class ViewTopupMemoryDataSyncQueue
{
    public int QueueId { get; set; }

    public int? SchoolId { get; set; }

    public string? UserId { get; set; }

    public bool? Processed { get; set; }

    public DateTime? CreatedAt { get; set; }
}
