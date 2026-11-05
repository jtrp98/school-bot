using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TgradeViewRoomListSetting
{
    public int RoomListSettingId { get; set; }

    public int GradeViewSettingId { get; set; }

    public int? NTermSubLevel2 { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public int? ApprovedBy { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int SchoolId { get; set; }

    public bool CDel { get; set; }

    public bool? IsRoomBlocked { get; set; }
}
