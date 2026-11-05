using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class Tdepartment
{
    public int DepId { get; set; }

    public int? UserHeadId { get; set; }

    public int? UserApproveOne { get; set; }

    public int? UserApproveTwo { get; set; }

    public string? DepartmentName { get; set; }

    public int? Deleted { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
