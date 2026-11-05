using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TmasterDbuser
{
    public int SId { get; set; }

    public int? NSystemId { get; set; }

    public int? NCompany { get; set; }

    public string? Nfc { get; set; }

    public char? CDel { get; set; }

    public string? Username { get; set; }

    public string? Userpassword { get; set; }

    public string? CType { get; set; }

    public decimal? NMoney { get; set; }

    public string? SName { get; set; }

    public string? SLastname { get; set; }

    public string? SIdentification { get; set; }

    public string? Pin { get; set; }

    public string? SPicture { get; set; }

    public string? PasswordHash { get; set; }

    public string? SEmail { get; set; }

    public int? NStudentStatus { get; set; }

    public bool? UseEncryptPassword { get; set; }

    public string? SStudentId { get; set; }
}
