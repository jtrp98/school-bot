using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class Tuser
{
    public int SId { get; set; }

    public string? SStudentTitle { get; set; }

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

    public decimal? NMax { get; set; }

    public char? CType { get; set; }

    public char? CDel { get; set; }

    public char? CSms { get; set; }

    public int? BaseSalary { get; set; }

    public string? CTelSms { get; set; }

    public int? NTermSubLevel2 { get; set; }

    public string? SToken { get; set; }

    public string? SStudentId { get; set; }

    public string? SStudentNameEn { get; set; }

    public string? SStudentLastEn { get; set; }

    public string? SStudentRace { get; set; }

    public string? SStudentNation { get; set; }

    public string? SStudentReligion { get; set; }

    public string? SStudentIdCardNumber { get; set; }

    public string? SStudentHomeNumber { get; set; }

    public string? SStudentSoy { get; set; }

    public string? SStudentTumbon { get; set; }

    public string? SStudentProvince { get; set; }

    public string? SStudentMuu { get; set; }

    public string? SStudentRoad { get; set; }

    public string? SStudentAumpher { get; set; }

    public string? SStudentPost { get; set; }

    public string? SStudentPicture { get; set; }

    public string? SNickName { get; set; }

    public int? NSonNumber { get; set; }

    public DateTime? DPicUpdate { get; set; }

    public int? NPicversion { get; set; }

    public int? NStudentNumber { get; set; }

    public int? NStudentStatus { get; set; }

    public string? ContactPeak { get; set; }

    public DateOnly? DayQuit { get; set; }

    public string? Note { get; set; }

    public string? OldSchoolName { get; set; }

    public string? OldSchoolProvince { get; set; }

    public string? OldSchoolAumpher { get; set; }

    public string? OldSchoolTumbon { get; set; }

    public decimal? OldSchoolGpa { get; set; }

    public string? OldSchoolGraduated { get; set; }

    public DateTime? MoveInDate { get; set; }

    public double? AddressLng { get; set; }

    public double? AddressLat { get; set; }

    public string? MoveOutReason { get; set; }

    public string? SNickNameEn { get; set; }

    public string? SStudentHomeRegisterCode { get; set; }

    public string? SStudentHousePhone { get; set; }

    public string? SStudentNameOther { get; set; }

    public string? SStudentLastOther { get; set; }

    public int SchoolId { get; set; }

    public string? Note2 { get; set; }

    public decimal? Credit { get; set; }

    public string? DiplomaCode { get; set; }

    public int? TranscriptSetNumber { get; set; }

    public int? TranscriptNumber { get; set; }

    public string? OldSchoolGpa2 { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? DisabilityCode { get; set; }

    public string? DisadvantageCode { get; set; }

    public decimal DailyUsageLimit { get; set; }

    public int? JourneyType { get; set; }

    public string? DormitoryName { get; set; }

    public DateTime? OldSchoolDateGraduated { get; set; }

    public string? PassportNumber { get; set; }

    public string? PassportCountry { get; set; }

    public DateTime? PassportExpirationDate { get; set; }

    public string? CountryType { get; set; }

    public string? CountryNameTh { get; set; }

    public string? CountryNameEn { get; set; }

    public int? StudentTypeId { get; set; }

    public string? ParentCode { get; set; }

    public string? OldSchoolName2 { get; set; }

    public decimal? OldSchoolGpa3 { get; set; }

    public decimal? Credit2 { get; set; }

    public DateTime? OldSchoolDateGraduated2 { get; set; }

    public string? OldSchoolGraduated2 { get; set; }

    public string? MoveOutReason2 { get; set; }

    public string? OldSchoolGpa4 { get; set; }

    public int? SourceTypeCode { get; set; }

    public long? ImportDocumentId { get; set; }

    public string? AccountNumber { get; set; }
}
