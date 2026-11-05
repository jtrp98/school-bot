using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TuserVerify
{
    public int VerifyId { get; set; }

    public int SchoolId { get; set; }

    public int UserId { get; set; }

    public int Type { get; set; }

    public int No { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? RequestNo { get; set; }

    public string? Token { get; set; }

    public string? EmailPin { get; set; }

    public string? Ref { get; set; }

    public DateTime? RequestDate { get; set; }

    public string? RequestResponse { get; set; }

    public string? VerifyPin { get; set; }

    public DateTime? VerifyDate { get; set; }

    public string? VerifyResponse { get; set; }

    public int Status { get; set; }
}
