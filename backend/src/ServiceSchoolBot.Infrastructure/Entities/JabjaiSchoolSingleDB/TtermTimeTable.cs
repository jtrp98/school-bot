using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TtermTimeTable
{
    public int NTermTable { get; set; }

    public int? NTermSubLevel2 { get; set; }

    public int? NTeacher { get; set; }

    public string? NTerm { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public int? PlanId { get; set; }
}
