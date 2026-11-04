using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbIpblockList
{
    public long Id { get; set; }

    public string Ipaddress { get; set; } = null!;

    public int? SchoolId { get; set; }

    public string BlockReason { get; set; } = null!;

    public string BlockType { get; set; } = null!;

    public DateTime BlockedFrom { get; set; }

    public DateTime? BlockedUntil { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public long? RelatedIncidentId { get; set; }
}
