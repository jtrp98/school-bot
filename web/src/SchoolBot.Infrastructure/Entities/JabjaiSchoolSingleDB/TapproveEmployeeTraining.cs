using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TapproveEmployeeTraining
{
    public int Id { get; set; }

    public int? Version { get; set; }

    public int EmployeeId { get; set; }

    public int SchoolId { get; set; }

    public int? No { get; set; }

    public int? Type { get; set; }

    public string? Project { get; set; }

    public string? TrainingCourseName { get; set; }

    public DateTime? TrainingStartDate { get; set; }

    public DateTime? TrainingEndDate { get; set; }

    public decimal? NumberHours { get; set; }

    public string? Location { get; set; }

    public string? Country { get; set; }

    public string? Province { get; set; }

    public decimal? Expenses { get; set; }

    public DateTime? RequestApproveDate { get; set; }

    public DateTime? ApproveDate { get; set; }

    public int? ApproveBy { get; set; }

    public string? ApproveStatus { get; set; }

    public string? ApproveComment { get; set; }

    public bool? UserAcknowledge { get; set; }

    public DateTime? EndDateShowResult { get; set; }
}
