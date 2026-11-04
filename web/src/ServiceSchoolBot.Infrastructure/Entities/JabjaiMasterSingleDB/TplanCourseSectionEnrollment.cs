using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TplanCourseSectionEnrollment
{
    public int PlanCourseEnrollmentId { get; set; }

    public int SchoolId { get; set; }

    public int SId { get; set; }

    public int PlanCourseSectionId { get; set; }

    public string? EnrollStatus { get; set; }

    public int? ApprovedByEmp { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? CDel { get; set; }
}
