using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class VMasterTuserNotUsed
{
    public int? SId { get; set; }

    public string? SName { get; set; }

    public string? SLastname { get; set; }

    public string? SIdentification { get; set; }

    public DateTime? DUpdatePass { get; set; }

    public string? Username { get; set; }

    public string? Userpassword { get; set; }

    public DateTime? DBirth { get; set; }

    public char? CSex { get; set; }

    public string? SPhone { get; set; }

    public string? SEmail { get; set; }

    public string? SPassword { get; set; }

    public string? SAddress { get; set; }

    public int? ProvinceId { get; set; }

    public int? AmphurId { get; set; }

    public int? DistrictId { get; set; }

    public int? NSystemId { get; set; }

    public string? SCity { get; set; }

    public string? SPostalcode { get; set; }

    public string? SCountry { get; set; }

    public DateTime? DCreate { get; set; }

    public DateTime? DUpdate { get; set; }

    public string? SFinger { get; set; }

    public string? SFinger2 { get; set; }

    public string? SFinger3 { get; set; }

    public string? Nfc { get; set; }

    public char? CType { get; set; }

    public char? CDel { get; set; }

    public int? NCompany { get; set; }

    public string? ContactPeak { get; set; }

    public string? Pin { get; set; }

    public string? Token { get; set; }

    public string? SecretKey { get; set; }

    public decimal? NMoney { get; set; }

    public Guid? UId { get; set; }

    public string? UserSignature { get; set; }

    public bool? UseBiometric { get; set; }

    public string? SPicture { get; set; }

    public string? LineId { get; set; }

    public string? PasswordHash { get; set; }

    public bool? UseEncryptPassword { get; set; }

    public bool? IsFirstLogin { get; set; }

    public int? CurrentFailedAttempts { get; set; }

    public DateTime? AccountLockedUntil { get; set; }

    public bool? IsAccountVerified { get; set; }

    public int? NStudentStatus { get; set; }
}
