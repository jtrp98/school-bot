using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TapproveEmployeeHistoryReceivingRoyalDecoration
{
    public int Id { get; set; }

    public int? Version { get; set; }

    public int EmployeeId { get; set; }

    public int SchoolId { get; set; }

    public int? No { get; set; }

    public int? YearReceived { get; set; }

    public string? RoyalDecorationClass { get; set; }

    public string? Position { get; set; }

    public string? RoyalGovernmentVolumeNo { get; set; }

    public string? RoyalGovernmentPart { get; set; }

    public string? RoyalGovernmentNo { get; set; }

    public DateTime? Dated { get; set; }

    public DateTime? RequestApproveDate { get; set; }

    public DateTime? ApproveDate { get; set; }

    public int? ApproveBy { get; set; }

    public string? ApproveStatus { get; set; }

    public string? ApproveComment { get; set; }

    public bool? UserAcknowledge { get; set; }

    public DateTime? EndDateShowResult { get; set; }
}
