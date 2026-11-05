using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TapproveStudentProfile
{
    public int Id { get; set; }

    public int? Version { get; set; }

    public int StudentId { get; set; }

    public int SchoolId { get; set; }

    public char? Gender { get; set; }

    public int? Title { get; set; }

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public string? NameEn { get; set; }

    public string? SurnameEn { get; set; }

    public string? NameOther { get; set; }

    public string? SurnameOther { get; set; }

    public string? Nickname { get; set; }

    public string? NicknameEn { get; set; }

    public DateTime? BirthDay { get; set; }

    public string? Race { get; set; }

    public string? Nationality { get; set; }

    public string? Religion { get; set; }

    public string? Disability { get; set; }

    public string? Disadvantaged { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public int? NumberMemberInFamily { get; set; }

    public int? YouAreChildOfFamily { get; set; }

    public int? HaveBrotherStudyInSchool { get; set; }

    public string? Other { get; set; }

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

    public decimal? SpendingPerDay { get; set; }
}
