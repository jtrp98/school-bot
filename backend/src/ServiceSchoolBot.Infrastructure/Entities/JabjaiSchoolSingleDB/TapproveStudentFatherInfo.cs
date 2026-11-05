using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TapproveStudentFatherInfo
{
    public int Id { get; set; }

    public int? Version { get; set; }

    public int StudentId { get; set; }

    public int SchoolId { get; set; }

    public int? Title { get; set; }

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public string? NameEn { get; set; }

    public string? SurnameEn { get; set; }

    public DateTime? BirthDay { get; set; }

    public string? IdcardNumber { get; set; }

    public string? Race { get; set; }

    public string? Nationality { get; set; }

    public string? Religion { get; set; }

    public int? Education { get; set; }

    public string? HouseNo { get; set; }

    public string? Soi { get; set; }

    public string? Moo { get; set; }

    public string? Road { get; set; }

    public int? Province { get; set; }

    public int? District { get; set; }

    public int? SubDistrict { get; set; }

    public string? PostalCode { get; set; }

    public string? Career { get; set; }

    public double? MonthlyIncome { get; set; }

    public string? WorkPlaces { get; set; }

    public string? PhoneNumberHouse { get; set; }

    public string? PhoneNumberMobile { get; set; }

    public string? PhoneNumberWorkPlace { get; set; }

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
