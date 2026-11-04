using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TplanCourseStudent
{
    public int PlanCourseStudentId { get; set; }

    public int PlanCourseId { get; set; }

    public int SId { get; set; }

    public bool CDel { get; set; }

    public int? CreatedBy { get; set; }

    public int? Updatedby { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int SchoolId { get; set; }

    public string? NTerm { get; set; }

    public bool IsActive { get; set; }

    public int? NTermSubLevel2 { get; set; }

    public int NTsubLevel { get; set; }
}
