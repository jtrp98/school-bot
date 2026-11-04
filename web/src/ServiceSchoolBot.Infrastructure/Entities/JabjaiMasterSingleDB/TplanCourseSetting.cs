using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TplanCourseSetting
{
    public int PlanCourseSettingId { get; set; }

    public int SchoolId { get; set; }

    public int PlanCourseId { get; set; }

    public int? MaxStudentsPerCourse { get; set; }

    public int? MaxSectionsPerCourse { get; set; }

    public DateTime? RegistrationStart { get; set; }

    public DateTime? RegistrationEnd { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? CDel { get; set; }

    public string? NTerm { get; set; }

    public bool IsRegistrationOpen { get; set; }
}
