using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TinvoiceDetail
{
    public int InvoiceDetailId { get; set; }

    public int? InvoiceId { get; set; }

    public int? ProductId { get; set; }

    public string? EducationType { get; set; }

    public int? CurrentTerm { get; set; }

    public int? Year { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? DueDate { get; set; }

    public decimal? Price { get; set; }

    public int? StudentCount { get; set; }

    public decimal? TotalPrice { get; set; }

    public DateTime? InvoiceCreateDate { get; set; }

    public string? InvoiceUrl { get; set; }

    public string? InvoiceCode { get; set; }

    public string? InvoiceRef { get; set; }

    public DateTime? SentEmailDate { get; set; }

    public DateTime? SentLineNotificationDate { get; set; }

    public bool? IsSuccess { get; set; }

    public bool? RecordDelete { get; set; }

    public int? DeleteBy { get; set; }

    public string? RecieptRef { get; set; }

    public string? RecieptCode { get; set; }

    public string? RecieptUrl { get; set; }

    public DateTime? RecieptCreateDate { get; set; }

    public string? PayStatus { get; set; }

    public decimal? OverduePrice { get; set; }

    public DateTime? RecieptSentEmailDate { get; set; }

    public DateTime? RecieptSentLineNotificationDate { get; set; }

    public int? CreateBy { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? InvoiceType { get; set; }

    public DateTime? SentVoiceDate { get; set; }

    public DateTime? RecieptSentVoiceDate { get; set; }
}
