using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TplanCourseSection
{
    public int PlanCourseSectionId { get; set; }

    public int SchoolId { get; set; }

    public int PlanCourseId { get; set; }

    public string NTerm { get; set; } = null!;

    public string? SectionNumber { get; set; }

    public int SEmp { get; set; }

    public int? OptimumSize { get; set; }

    public int? PeriodCount { get; set; }

    public bool? IsLocked { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? CDel { get; set; }
}
