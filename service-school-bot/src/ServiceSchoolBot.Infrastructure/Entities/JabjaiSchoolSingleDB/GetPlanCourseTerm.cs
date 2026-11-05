using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class GetPlanCourseTerm
{
    public string? NTerm { get; set; }

    public string? STerm { get; set; }

    public int? NYear { get; set; }

    public int? SchoolId { get; set; }

    public int? PlanCourseId { get; set; }

    public bool? IsActive { get; set; }
}
