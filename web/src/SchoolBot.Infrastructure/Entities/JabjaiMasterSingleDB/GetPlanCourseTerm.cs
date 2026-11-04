using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class GetPlanCourseTerm
{
    public string? NTerm { get; set; }

    public string? STerm { get; set; }

    public int? NYear { get; set; }

    public int? SchoolId { get; set; }

    public int? PlanCourseId { get; set; }

    public bool? IsActive { get; set; }
}
