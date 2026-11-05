using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class Temployee
{
    public int SEmp { get; set; }

    public string? SName { get; set; }

    public string? SLastname { get; set; }

    public string? SIdentification { get; set; }

    public DateTime? DBirth { get; set; }

    public char? CSex { get; set; }

    public string? SPhone { get; set; }

    public string? SEmail { get; set; }

    public string? SPassword { get; set; }

    public string? SSubtopic { get; set; }

    public string? SAddress { get; set; }

    public string? SCity { get; set; }

    public string? SPostalcode { get; set; }

    public string? SCountry { get; set; }

    public DateTime? DUpdate { get; set; }

    public string? SFinger { get; set; }

    public decimal? NMoney { get; set; }

    public string? SFinger2 { get; set; }

    public string? SClaim { get; set; }

    public char? CDel { get; set; }

    public string? SStatusReport { get; set; }

    public int? NTimeType { get; set; }

    public string? CType { get; set; }

    public string? SToken { get; set; }

    public string? SPicture { get; set; }

    public string? SProvince { get; set; }

    public string? STumbon { get; set; }

    public string? SSoy { get; set; }

    public string? SHomeNumber { get; set; }

    public string? SMuu { get; set; }

    public string? SRoad { get; set; }

    public string? SAumpher { get; set; }

    public string? SPost { get; set; }

    public string? STitle { get; set; }

    public DateTime? DPicUpdate { get; set; }

    public int? NPicversion { get; set; }

    public int? Leavecheck { get; set; }

    public int? NJobid { get; set; }

    public int? NDepartmentId { get; set; }

    public int? GradeSystemAdmin { get; set; }

    public string? Village { get; set; }

    public string? Building { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public decimal DailyUsageLimit { get; set; }

    public decimal NMax { get; set; }

    public string? SNameEng { get; set; }

    public string? SLastNameEng { get; set; }

    public int? SourceTypeCode { get; set; }

    public long? ImportDocumentId { get; set; }

    public string? SAutoBillingEmail { get; set; }
}
