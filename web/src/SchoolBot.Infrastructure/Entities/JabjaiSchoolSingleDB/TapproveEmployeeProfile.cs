using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TapproveEmployeeProfile
{
    public int Id { get; set; }

    public int? Version { get; set; }

    public int EmployeeId { get; set; }

    public int SchoolId { get; set; }

    public char? Gender { get; set; }

    public int? Title { get; set; }

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public string? NameEn { get; set; }

    public string? SurnameEn { get; set; }

    public string? IdcardNumber { get; set; }

    public string? PassportNumber { get; set; }

    public string? PassportCountry { get; set; }

    public DateTime? BirthDay { get; set; }

    public string? BloodType { get; set; }

    public string? Nationality { get; set; }

    public string? Race { get; set; }

    public string? Religion { get; set; }

    public int? MaritalStatus { get; set; }

    public string? SpouseName { get; set; }

    public string? SpouseSurname { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public DateTime? RequestApproveDate { get; set; }

    public DateTime? ApproveDate { get; set; }

    public int? ApproveBy { get; set; }

    public string? ApproveStatus { get; set; }

    public string? ApproveComment { get; set; }

    public bool? UserAcknowledge { get; set; }

    public DateTime? EndDateShowResult { get; set; }
}
