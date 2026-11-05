using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TempSalary
{
    public int SEmp { get; set; }

    public int Id { get; set; }

    public int? WorkStatus { get; set; }

    public DateTime? WorkInEducationDate { get; set; }

    public decimal? Salary { get; set; }

    public decimal? PositionMoney { get; set; }

    public DateTime? RetirementDate { get; set; }

    public int? RemainGovernmentYear { get; set; }

    public int? RemainGovernmentMonth { get; set; }

    public int? RemainGovernmentDay { get; set; }

    public string? Degree { get; set; }

    public DateTime? GovernmentOrderDate { get; set; }

    public DateTime? WorkStartDate { get; set; }

    public decimal? AcademicStandingMoney { get; set; }

    public decimal? NetSalary { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public DateOnly? DayQuit { get; set; }
}
