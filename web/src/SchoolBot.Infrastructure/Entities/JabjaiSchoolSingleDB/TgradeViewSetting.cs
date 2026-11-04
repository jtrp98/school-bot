using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TgradeViewSetting
{
    public int GradeViewSettingId { get; set; }

    public string NTerm { get; set; } = null!;

    public bool? IsTermApproved { get; set; }

    public bool? IsAllRoomApproved { get; set; }

    public int SchoolId { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool CDel { get; set; }

    public bool GradeViewFor100 { get; set; }

    public bool IsMidTermApproved { get; set; }

    public bool IsFinalTermApproved { get; set; }

    public bool IsBeforeMidTermApproved { get; set; }

    public bool IsAfterMidTermApproved { get; set; }
}
