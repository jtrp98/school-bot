using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tinvoice
{
    public int InvoiceId { get; set; }

    public int? SchoolId { get; set; }

    public string? PriceType { get; set; }

    public string? CalculationType { get; set; }

    public string? ServiceType { get; set; }

    public int? CurrentYear { get; set; }

    public int? TaxStatus { get; set; }

    public int? Term1Day { get; set; }

    public int? Term1Month { get; set; }

    public int? Term2Day { get; set; }

    public int? Term2Month { get; set; }

    public int? YearDay { get; set; }

    public int? YearMonth { get; set; }

    public DateTime? IssueDate { get; set; }

    public DateTime? DueDate { get; set; }

    public int? NoOfDueDate { get; set; }

    public int? ContactId { get; set; }

    public string? Note { get; set; }

    public int? StudentQuantity { get; set; }

    public int? StudentAllowUsing { get; set; }

    public int? StudentPay { get; set; }

    public decimal? TotalPrice { get; set; }

    public DateTime? InvoiceCreateDate { get; set; }

    public string? InvoiceUrl { get; set; }

    public string? InvoiceCode { get; set; }

    public string? InvoiceRef { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsSuccess { get; set; }

    public DateTime? SentEmailDate { get; set; }

    public DateTime? SentLineNotificationDate { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public bool? RecordDelete { get; set; }

    public int? DeleteBy { get; set; }

    public string? PeakModel { get; set; }

    public string? SettingModel { get; set; }

    public string? VoiceModel { get; set; }
}
