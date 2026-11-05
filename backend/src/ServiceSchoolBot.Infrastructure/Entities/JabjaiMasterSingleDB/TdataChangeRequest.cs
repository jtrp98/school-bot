using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TdataChangeRequest
{
    public long RequestId { get; set; }

    public string ActionType { get; set; } = null!;

    public int SchoolId { get; set; }

    public string? SchoolName { get; set; }

    public string PayloadJson { get; set; } = null!;

    public string? Summary { get; set; }

    public string Status { get; set; } = null!;

    public int CreateBy { get; set; }

    public string? CreateByName { get; set; }

    public DateTime CreateDate { get; set; }

    public int? ApproveBy { get; set; }

    public string? ApproveByName { get; set; }

    public DateTime? ApproveDate { get; set; }

    public string? RejectReason { get; set; }
}
