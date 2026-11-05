using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TapproveStudentContactAddress
{
    public int Id { get; set; }

    public int? Version { get; set; }

    public int StudentId { get; set; }

    public int SchoolId { get; set; }

    public bool? IsPermanentAddress { get; set; }

    public string? HouseNo { get; set; }

    public string? Soi { get; set; }

    public string? Moo { get; set; }

    public string? Road { get; set; }

    public int? Province { get; set; }

    public int? District { get; set; }

    public int? SubDistrict { get; set; }

    public string? PostalCode { get; set; }

    public string? HousePhone { get; set; }

    public int? LiveWithTitle { get; set; }

    public string? LiveWithName { get; set; }

    public string? LiveWithSurname { get; set; }

    public string? EmergencyPhone { get; set; }

    public string? LiveWithEmail { get; set; }

    public string? NeighborName { get; set; }

    public string? NeighborSurname { get; set; }

    public string? NeighborPhone { get; set; }

    public int? HouseStyle { get; set; }

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
