using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TemployeeInfo
{
    public int SEmp { get; set; }

    public int Id { get; set; }

    public string? Code { get; set; }

    public string? FirstNameEn { get; set; }

    public string? LastNameEn { get; set; }

    public string? PassportNumber { get; set; }

    public string? PassportCountry { get; set; }

    public DateTime? PassportExpirationDate { get; set; }

    public string? VisaNo { get; set; }

    public DateTime? VisaExpirationDate { get; set; }

    public string? WorkPermitNo { get; set; }

    public DateTime? WorkPermitExpirationDate { get; set; }

    public string? BloodType { get; set; }

    public string? Nationality { get; set; }

    public string? Ethnicity { get; set; }

    public string? Religion { get; set; }

    public int? PersonalStatus { get; set; }

    public string? SpouseFirstName { get; set; }

    public string? SpouseLastName { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool CDel { get; set; }

    public int? SourceTypeCode { get; set; }

    public long? ImportDocumentId { get; set; }
}
