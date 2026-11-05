using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TpreRegister
{
    public int PreRegisterId { get; set; }

    public int? StudentTitle { get; set; }

    public string? SName { get; set; }

    public string? SLastname { get; set; }

    public string? SIdentification { get; set; }

    public DateTime? DBirth { get; set; }

    public char? CSex { get; set; }

    public string? SPhone { get; set; }

    public string? SEmail { get; set; }

    public string? SAddress { get; set; }

    public string? SCity { get; set; }

    public string? SPostalcode { get; set; }

    public string? SCountry { get; set; }

    public char? CType { get; set; }

    public int? CDel { get; set; }

    public int? NTermSubLevel2 { get; set; }

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

    public string? OldSchoolName { get; set; }

    public string? OldSchoolProvince { get; set; }

    public string? OldSchoolAumpher { get; set; }

    public string? OldSchoolTumbon { get; set; }

    public double? OldSchoolGpa { get; set; }

    public string? OldSchoolGraduated { get; set; }

    public int? NFamilyTitle { get; set; }

    public string? SFamilyName { get; set; }

    public string? SFamilyLast { get; set; }

    public string? SFamilyRace { get; set; }

    public string? SFamilyNation { get; set; }

    public string? SFamilyReligion { get; set; }

    public string? SFamilyIdCardNumber { get; set; }

    public string? SFamilyRelate { get; set; }

    public string? SFamilyHomeNumber { get; set; }

    public string? SFamilySoy { get; set; }

    public string? SFamilyTumbon { get; set; }

    public string? SFamilyProvince { get; set; }

    public string? SFamilyMuu { get; set; }

    public string? SFamilyRoad { get; set; }

    public string? SFamilyAumpher { get; set; }

    public string? SFamilyPost { get; set; }

    public string? SPhoneOne { get; set; }

    public string? SPhoneTwo { get; set; }

    public string? SPhoneThree { get; set; }

    public string? SPhoneMail { get; set; }

    public int? FatherTitle { get; set; }

    public string? SFatherFirstName { get; set; }

    public string? SFatherLastName { get; set; }

    public string? SFatherNation { get; set; }

    public string? SFatherRace { get; set; }

    public string? SFatherReligion { get; set; }

    public string? SFatherIdCardNumber { get; set; }

    public string? SFatherHomeNumber { get; set; }

    public string? SFatherSoy { get; set; }

    public string? SFatherTumbon { get; set; }

    public string? SFatherProvince { get; set; }

    public string? SFatherMuu { get; set; }

    public string? SFatherRoad { get; set; }

    public string? SFatherAumpher { get; set; }

    public string? SFatherPost { get; set; }

    public string? SFatherPhone { get; set; }

    public int? MotherTitle { get; set; }

    public string? SMotherFirstName { get; set; }

    public string? SMotherLastName { get; set; }

    public string? SMotherNation { get; set; }

    public string? SMotherRace { get; set; }

    public string? SMotherReligion { get; set; }

    public string? SMotherIdCardNumber { get; set; }

    public string? SMotherHomeNumber { get; set; }

    public string? SMotherSoy { get; set; }

    public string? SMotherTumbon { get; set; }

    public string? SMotherProvince { get; set; }

    public string? SMotherMuu { get; set; }

    public string? SMotherRoad { get; set; }

    public string? SMotherAumpher { get; set; }

    public string? SMotherPost { get; set; }

    public string? SMotherPhone { get; set; }

    public double? NWeight { get; set; }

    public double? NHeight { get; set; }

    public string? SBlood { get; set; }

    public string? SSickFood { get; set; }

    public string? SSickDrug { get; set; }

    public string? SSickOther { get; set; }

    public string? SSickNormal { get; set; }

    public string? SSickDanger { get; set; }

    public int RegisterStatus { get; set; }

    public int PaymentStatus { get; set; }

    public int RegisterYear { get; set; }

    public string RegisterCode { get; set; } = null!;

    public double? AddressLat { get; set; }

    public double? AddressLng { get; set; }

    public DateTime? MoveInDate { get; set; }

    public int? OptionCourse { get; set; }

    public int? OptionLevel { get; set; }

    public int? OptionTime { get; set; }

    public int? OptionBranch { get; set; }

    public int? SaveAsSid { get; set; }

    public string? KnowFrom9txt { get; set; }

    public int? KnowFrom9 { get; set; }

    public string? KnowFrom8txt { get; set; }

    public int? KnowFrom8 { get; set; }

    public string? KnowFrom7txt { get; set; }

    public int? KnowFrom7 { get; set; }

    public int? KnowFrom6 { get; set; }

    public string? KnowFrom5txt { get; set; }

    public int? KnowFrom5 { get; set; }

    public int? KnowFrom4 { get; set; }

    public int? KnowFrom3 { get; set; }

    public int? KnowFrom2 { get; set; }

    public int? KnowFrom1 { get; set; }

    public string? MotherIncome { get; set; }

    public string? FatherIncome { get; set; }

    public int? KnowFrom10 { get; set; }

    public int? KnowFrom11 { get; set; }

    public DateTime? AddDate { get; set; }

    public string? SFamilyNameEn { get; set; }

    public string? SFamilyLastEn { get; set; }

    public DateTime? DFamilyBirthDay { get; set; }

    public int? NFamilyRequestStudyMoney { get; set; }

    public int? SFamilyGraduated { get; set; }

    public string? SFamilyJob { get; set; }

    public string? SFamilyWorkPlace { get; set; }

    public double? NFamilyIncome { get; set; }

    public string? SFatherNameEn { get; set; }

    public string? SFatherLastEn { get; set; }

    public DateTime? DFatherBirthDay { get; set; }

    public int? SFatherGraduated { get; set; }

    public string? SFatherJob { get; set; }

    public string? SFatherWorkPlace { get; set; }

    public string? SFatherPhone2 { get; set; }

    public string? SFatherPhone3 { get; set; }

    public double? NFatherIncome { get; set; }

    public string? SMotherNameEn { get; set; }

    public string? SMotherLastEn { get; set; }

    public DateTime? DMotherBirthDay { get; set; }

    public int? SMotherGraduated { get; set; }

    public string? SMotherJob { get; set; }

    public string? SMotherWorkPlace { get; set; }

    public string? SMotherPhone2 { get; set; }

    public string? SMotherPhone3 { get; set; }

    public double? NMotherIncome { get; set; }

    public int? NSonTotal { get; set; }

    public int? NRelativeStudyHere { get; set; }

    public string? SNickNameEn { get; set; }

    public string? SStudentHomeRegisterCode { get; set; }

    public int? StayWithTitle { get; set; }

    public string? StayWithName { get; set; }

    public string? StayWithLast { get; set; }

    public string? StayWithEmergencyCall { get; set; }

    public string? StayWithEmail { get; set; }

    public int? HomeType { get; set; }

    public int? FriendSid { get; set; }

    public string? HouseRegistrationNumber { get; set; }

    public string? HouseRegistrationMuu { get; set; }

    public string? HouseRegistrationSoy { get; set; }

    public string? HouseRegistrationRoad { get; set; }

    public int? HouseRegistrationProvince { get; set; }

    public int? HouseRegistrationAumpher { get; set; }

    public int? HouseRegistrationTumbon { get; set; }

    public string? HouseRegistrationPost { get; set; }

    public string? HouseRegistrationPhone { get; set; }

    public string? BornFrom { get; set; }

    public int? BornFromProvince { get; set; }

    public int? BornFromAumpher { get; set; }

    public int? BornFromTumbon { get; set; }

    public string? MoveOutReason { get; set; }

    public string? SStudentHousePhone { get; set; }

    public string? FriendName { get; set; }

    public string? FriendLastName { get; set; }

    public int? FriendSubLevel { get; set; }

    public string? FriendPhone { get; set; }

    public int? FamilyStatus { get; set; }

    public string? SStudentNameOther { get; set; }

    public string? SStudentLastOther { get; set; }

    public string? StudentType { get; set; }

    public int? RegisterPlanSetupId { get; set; }

    public string? ExamCode { get; set; }

    public string? StudentCategory { get; set; }

    public string? FatherAnnualIncome { get; set; }

    public string? FatherEmail { get; set; }

    public string? MotherAnnualIncome { get; set; }

    public string? MotherEmail { get; set; }

    public string? ParentAnnualIncome { get; set; }

    public string? ParentEmail { get; set; }

    public string? ExamResults { get; set; }

    public int? ExamResultsUpdateBy { get; set; }

    public DateTime? ExamResultsUpdateDate { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? ExamSeatNo { get; set; }

    public int? RegisterExamRoomId { get; set; }

    public string? CompleteDocuments { get; set; }

    public int? CompleteDocumentsUpdateBy { get; set; }

    public DateTime? CompleteDocumentsUpdateDate { get; set; }

    public string? CompleteDocumentsInfo { get; set; }

    public int? MainPlan { get; set; }

    public string? BackupPlans { get; set; }

    public int? ReserveFromSid { get; set; }

    public string? NTerm { get; set; }

    public string? DisabilityCode { get; set; }

    public bool? HasDisabilityCard { get; set; }

    public string? DisabilityCardNo { get; set; }

    public DateTime? DisabilityCardExpireDate { get; set; }

    public bool? HasDisabilityCert { get; set; }

    public DateTime? DisabilityCertIssueDate { get; set; }

    public int? RegisterProgramSetupId { get; set; }
}
