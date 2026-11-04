using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class Tschedule
{
    public int SScheduleId { get; set; }

    public int? NPlaneDay { get; set; }

    public TimeOnly? TStart { get; set; }

    public TimeOnly? TEnd { get; set; }

    public TimeOnly? DTimeStartIn { get; set; }

    public TimeOnly? DTimeStartOut { get; set; }

    public TimeOnly? DTimeEndIn { get; set; }

    public TimeOnly? DTimeEndOut { get; set; }

    public int? NTimeLate { get; set; }

    public TimeOnly? DTimeHalf { get; set; }

    public string? SClassId { get; set; }

    public int? SPlaneId { get; set; }

    public int? SEmp { get; set; }

    public int? NTermTable { get; set; }

    public bool? CActive { get; set; }

    public bool? CDel { get; set; }

    public bool? Calculate { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? CourseAbbreviationId { get; set; }

    public int? PlanCourseSectionPeriodId { get; set; }
}
