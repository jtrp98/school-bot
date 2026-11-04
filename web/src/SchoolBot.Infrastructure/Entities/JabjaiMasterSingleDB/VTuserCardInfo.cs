using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class VTuserCardInfo
{
    public int? UserId { get; set; }

    public char? CType { get; set; }

    public string? SName { get; set; }

    public string? SLastname { get; set; }

    public int? SchoolId { get; set; }

    public decimal? NMoney { get; set; }

    public string? UserName { get; set; }

    public string? BarCode { get; set; }

    public string? _1 { get; set; }

    public string? _2 { get; set; }

    public string? _3 { get; set; }

    public string? S1 { get; set; }

    public string? S2 { get; set; }

    public string? S3 { get; set; }

    public string? N1 { get; set; }

    public string? N2 { get; set; }

    public string? N3 { get; set; }

    public string? Nr1 { get; set; }

    public string? Nr2 { get; set; }

    public string? Nr3 { get; set; }

    public string? Nfcreverse1 { get; set; }

    public string? Nfcreverse2 { get; set; }

    public string? Nfcreverse3 { get; set; }
}
