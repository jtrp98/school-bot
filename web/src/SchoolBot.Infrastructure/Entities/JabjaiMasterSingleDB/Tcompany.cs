using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tcompany
{
    public int NCompany { get; set; }

    public string? SCompany { get; set; }

    public string? SSchoolPass { get; set; }

    public string? SAddress { get; set; }

    public string? STel { get; set; }

    public string? SServer { get; set; }

    public string? SDatabases { get; set; }

    public string? SUser { get; set; }

    public string? SPassword { get; set; }

    public string? SEntities { get; set; }

    public char? NType { get; set; }

    public string? SImage { get; set; }

    public int? Admin1 { get; set; }

    public int? Admin2 { get; set; }

    public bool? SingleAuthen { get; set; }

    public bool? DoubleAuthen { get; set; }

    public bool? Nfc { get; set; }

    public bool? Sotfware { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? SchoolHeadPicture { get; set; }

    public string? ServerAdminPicture { get; set; }

    public string? SchoolCoverPicture { get; set; }

    public string? SCode { get; set; }

    public string? SNameEn { get; set; }

    public string? SPhoneOne { get; set; }

    public string? SPhoneTwo { get; set; }

    public string? SMobilePhone { get; set; }

    public string? SFax { get; set; }

    public string? SEmailOne { get; set; }

    public string? SEmailTwo { get; set; }

    public string? SWebsite { get; set; }

    public string? SHomeNumber { get; set; }

    public string? SSoy { get; set; }

    public string? SMuu { get; set; }

    public string? SRoad { get; set; }

    public string? STumbon { get; set; }

    public string? SAumpher { get; set; }

    public string? SPost { get; set; }

    public string? SchoolHeadName { get; set; }

    public string? SchoolHeadLastname { get; set; }

    public string? SchoolHeadEmail { get; set; }

    public string? SchoolHeadPhone { get; set; }

    public string? ServerAdminName { get; set; }

    public string? ServerAdminLastname { get; set; }

    public string? ServerAdminEmail { get; set; }

    public string? ServerAdminPhone { get; set; }

    public int? Checker { get; set; }

    public string? SProvince { get; set; }

    public string? SOwner { get; set; }

    public bool? Active { get; set; }

    public int? SettingTimePeriod { get; set; }

    public int? SettingPlanTeacher { get; set; }

    public int? SettingGradeAdmin { get; set; }

    public int? SettingReportView { get; set; }

    public string? TaxId { get; set; }

    public int? NSchoolHeadid { get; set; }

    public int? NAcademicDirectorid { get; set; }

    public int? NRegistraDirectorid { get; set; }

    public int? NAcademicSubDirectorid { get; set; }

    public int? NAccountingDirectorid { get; set; }

    public int? NStudentDevelopmentDirectorid { get; set; }

    public int? NWebAdminid { get; set; }

    public int? NGm { get; set; }

    public int? NPersonnel { get; set; }

    public string? SShortSchoolName { get; set; }

    public string? SSchoolHistory { get; set; }

    public string? SSchoolVision { get; set; }

    public string? SSchoolMission { get; set; }

    public string? EducationArea { get; set; }

    public Guid UId { get; set; }

    public string? PaymentApiurl { get; set; }

    public bool? IsActive { get; set; }

    public bool? CDel { get; set; }

    public decimal? CostGradeRepairMid { get; set; }

    public decimal? CostGradeRepair { get; set; }

    public decimal? CostGradeRepairFinal { get; set; }

    public string? SchoolAreaCode { get; set; }

    public bool GradeViewFor100 { get; set; }

    public bool GradeViewAutoBlock { get; set; }

    public string? SAumpherEng { get; set; }

    public string? SProvinceEng { get; set; }

    public string? SchoolClass { get; set; }

    public int? UserSupportId { get; set; }

    public int? UserSaleId { get; set; }

    public string? SchoolGrade { get; set; }

    public DateTime? ActiveDate { get; set; }

    public DateTime? InactiveDate { get; set; }

    public bool? IsActiveSendMessageToLine { get; set; }

    public short? TumbonId { get; set; }

    public short? AumpherId { get; set; }

    public short? ProvinceId { get; set; }

    public bool? ClassNameDisable { get; set; }

    public char? CrmGrade { get; set; }

    public string? SchoolDataType { get; set; }

    public int? ActiveStatus { get; set; }
}
