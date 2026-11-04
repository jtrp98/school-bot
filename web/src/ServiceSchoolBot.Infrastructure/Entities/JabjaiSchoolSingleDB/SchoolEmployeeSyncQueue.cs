using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class SchoolEmployeeSyncQueue
{
    public int QueueId { get; set; }

    public int? SEmp { get; set; }

    public int? SchoolId { get; set; }

    public bool? Processed { get; set; }

    public DateTime? CreatedAt { get; set; }
}
