using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TplanCourse
{
    public int PlanCourseId { get; set; }

    public int PlanId { get; set; }

    public int SPlaneId { get; set; }

    public double? NCredit { get; set; }

    public double? CourseHour { get; set; }

    public double? CourseTotalHour { get; set; }

    public int? CourseStatus { get; set; }

    public int? SortNumber { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public bool? IsActive { get; set; }

    public int SchoolId { get; set; }

    public int? NYear { get; set; }

    public int? Rb1displayOrder { get; set; }

    public int? IsActiveForGrade { get; set; }

    public bool CDel { get; set; }

    public double? RatioQuiz { get; set; }

    public double? RatioBeforeMidTerm { get; set; }

    public double? RatioAfterMidTerm { get; set; }

    public double? RatioMidTerm { get; set; }

    public double? RatioLateTerm { get; set; }

    public double? RatioQuizPass { get; set; }

    public bool IsUserAllowedToEditRatio { get; set; }
}
