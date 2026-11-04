using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TfamilyProfile
{
    public string? SFamilyTitle { get; set; }

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

    public int? SId { get; set; }

    public string? SDeleted { get; set; }

    public string? SFatherTitle { get; set; }

    public string? SFatherFirstName { get; set; }

    public string? SFatherLastName { get; set; }

    public string? SFatherNation { get; set; }

    public string? SFatherRace { get; set; }

    public string? SFatherReligion { get; set; }

    public string? SFatherIdCardNumber { get; set; }

    public string? SMotherTitle { get; set; }

    public string? SMotherFirstName { get; set; }

    public string? SMotherLastName { get; set; }

    public string? SMotherNation { get; set; }

    public string? SMotherRace { get; set; }

    public string? SMotherReligion { get; set; }

    public string? SMotherIdCardNumber { get; set; }

    public string? SMotherTumbon { get; set; }

    public string? SMotherSoy { get; set; }

    public string? SMotherRoad { get; set; }

    public string? SMotherProvince { get; set; }

    public string? SMotherPost { get; set; }

    public string? SMotherPhone { get; set; }

    public string? SMotherMuu { get; set; }

    public string? SMotherHomeNumber { get; set; }

    public string? SMotherAumpher { get; set; }

    public string? SFatherTumbon { get; set; }

    public string? SFatherSoy { get; set; }

    public string? SFatherRoad { get; set; }

    public string? SFatherProvince { get; set; }

    public string? SFatherPost { get; set; }

    public string? SFatherPhone { get; set; }

    public string? SFatherMuu { get; set; }

    public string? SFatherHomeNumber { get; set; }

    public string? SFatherAumpher { get; set; }

    public int NFamilyId { get; set; }

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

    public string? FriendName { get; set; }

    public string? FriendLastName { get; set; }

    public int? FriendSubLevel { get; set; }

    public string? FriendPhone { get; set; }

    public int? FamilyStatus { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public string? BornFromEn { get; set; }
}
