using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TapproveEmployeeHonor
{
    public int Id { get; set; }

    public int? Version { get; set; }

    public int EmployeeId { get; set; }

    public int SchoolId { get; set; }

    public int? No { get; set; }

    public string? HonorType { get; set; }

    public string? DonatingAgency { get; set; }

    public int? YearReceived { get; set; }

    public DateTime? RequestApproveDate { get; set; }

    public DateTime? ApproveDate { get; set; }

    public int? ApproveBy { get; set; }

    public string? ApproveStatus { get; set; }

    public string? ApproveComment { get; set; }

    public bool? UserAcknowledge { get; set; }

    public DateTime? EndDateShowResult { get; set; }
}
