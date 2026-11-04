using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TplanCourseSectionAllowedRoom
{
    public int PlanCourseAllowedRoomId { get; set; }

    public int SchoolId { get; set; }

    public int PlanCourseSectionId { get; set; }

    public int NTermSubLevel2 { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? CDel { get; set; }
}
