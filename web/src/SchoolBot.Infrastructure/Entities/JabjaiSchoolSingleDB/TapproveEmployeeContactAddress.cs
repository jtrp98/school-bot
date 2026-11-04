using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TapproveEmployeeContactAddress
{
    public int Id { get; set; }

    public int? Version { get; set; }

    public int EmployeeId { get; set; }

    public int SchoolId { get; set; }

    public bool? IsPermanentAddress { get; set; }

    public string? HouseNo { get; set; }

    public string? Moo { get; set; }

    public string? Village { get; set; }

    public string? Soi { get; set; }

    public string? Building { get; set; }

    public string? Road { get; set; }

    public int? Province { get; set; }

    public int? District { get; set; }

    public int? SubDistrict { get; set; }

    public string? PostalCode { get; set; }

    public DateTime? RequestApproveDate { get; set; }

    public DateTime? ApproveDate { get; set; }

    public int? ApproveBy { get; set; }

    public string? ApproveStatus { get; set; }

    public string? ApproveComment { get; set; }

    public bool? UserAcknowledge { get; set; }

    public DateTime? EndDateShowResult { get; set; }
}
