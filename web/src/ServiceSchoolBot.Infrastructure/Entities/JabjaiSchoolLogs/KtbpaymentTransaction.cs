using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class KtbpaymentTransaction
{
    public int PaymentTransId { get; set; }

    public string? Ktbuser { get; set; }

    public string? Ktbpassword { get; set; }

    public string? ComCode { get; set; }

    public string? ProdCode { get; set; }

    public string? Command { get; set; }

    public int? BankCode { get; set; }

    public string? BankRef { get; set; }

    public string? TranxId { get; set; }

    public string? DateTime { get; set; }

    public string? EffDate { get; set; }

    public decimal? Amount { get; set; }

    public string? Channel { get; set; }

    public string? CusName { get; set; }

    public string? Ref1 { get; set; }

    public string? Ref2 { get; set; }

    public string? Ref3 { get; set; }

    public string? Ref4 { get; set; }

    public int SchoolId { get; set; }

    public int TransId { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? RespCode { get; set; }

    public string? RespMsg { get; set; }
}
