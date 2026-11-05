using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TgradeViewStudentBlockListSetting
{
    public int StudentBlockListSettingId { get; set; }

    public int GradeViewSettingId { get; set; }

    public int SId { get; set; }

    public DateTime? BlockedDate { get; set; }

    public int? BlockedBy { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public bool? IsStudentBlocked { get; set; }

    public int SchoolId { get; set; }
}
