using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TplanCourseSectionPeriod
{
    public int PlanCourseSectionPeriodId { get; set; }

    public int SchoolId { get; set; }

    public int PlanCourseSectionId { get; set; }

    public int? PeriodCapacity { get; set; }

    public double? NCredit { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? CDel { get; set; }
}
