using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TapproveStudentPermanentAddress
{
    public int Id { get; set; }

    public int? Version { get; set; }

    public int StudentId { get; set; }

    public int SchoolId { get; set; }

    public string? HouseCode { get; set; }

    public string? HouseNo { get; set; }

    public string? Soi { get; set; }

    public string? Moo { get; set; }

    public string? Road { get; set; }

    public int? Province { get; set; }

    public int? District { get; set; }

    public int? SubDistrict { get; set; }

    public string? PostalCode { get; set; }

    public string? HomePhoneNumber { get; set; }

    public string? BirthPlace { get; set; }

    public int? BirthPlaceProvince { get; set; }

    public int? BirthPlaceDistrict { get; set; }

    public int? BirthPlaceSubDistrict { get; set; }

    public DateTime? RequestApproveDate { get; set; }

    public DateTime? ApproveDate { get; set; }

    public int? ApproveBy { get; set; }

    public string? ApproveStatus { get; set; }

    public string? ApproveComment { get; set; }

    public bool? UserAcknowledge { get; set; }

    public DateTime? EndDateShowResult { get; set; }

    public string? RecoveryScript { get; set; }

    public DateTime? RecoveryDate { get; set; }

    public int? RecoveryBy { get; set; }
}
