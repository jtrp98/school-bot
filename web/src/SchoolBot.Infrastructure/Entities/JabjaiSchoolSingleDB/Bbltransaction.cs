using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class Bbltransaction
{
    public Guid TransactionId { get; set; }

    public string? PayeeId { get; set; }

    public string? TransDate { get; set; }

    public string? TransTime { get; set; }

    public string? TransRef { get; set; }

    public string? Channel { get; set; }

    public string? TermId { get; set; }

    public string? Amount { get; set; }

    public string? Reference1 { get; set; }

    public string? Reference2 { get; set; }

    public string? FromBank { get; set; }

    public string? RetryFlag { get; set; }

    public int? InvoiceId { get; set; }

    public int? SchoolId { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? VerifyDate { get; set; }

    public string? VerifyCode { get; set; }

    public string? VerifyStatus { get; set; }

    public DateTime? NotifyDate { get; set; }

    public string? NotifyCode { get; set; }

    public string? NotifyStatus { get; set; }
}
