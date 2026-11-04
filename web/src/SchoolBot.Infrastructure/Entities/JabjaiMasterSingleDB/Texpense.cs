using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Texpense
{
    public int ExpenseId { get; set; }

    public int? Year { get; set; }

    public int? Month { get; set; }

    public int? AccountChartId { get; set; }

    public string? AccountChartType { get; set; }

    public string? BillingType { get; set; }

    public string? IssueBy { get; set; }

    public DateTime? IssueDate { get; set; }

    public string? Description { get; set; }

    public int? Qty { get; set; }

    public decimal? PriceBeforeVat { get; set; }

    public decimal? Vat { get; set; }

    public decimal? TotalPrice { get; set; }

    public decimal? RawTotalPrice { get; set; }

    public string? AttachUrl { get; set; }

    public string? Status { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? DeleteDate { get; set; }

    public int? DeleteBy { get; set; }

    public DateTime? ApproveDate { get; set; }

    public int? ApproveBy { get; set; }

    public string? PaidType { get; set; }

    public string? VehicleCode { get; set; }

    public DateTime? CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    public string? ExpenseRefId { get; set; }

    public string? ExpenseCode { get; set; }

    public string? ExpenseUrl { get; set; }

    public DateTime? ExpenseCreateDate { get; set; }

    public string? ExpenseRefIdfleetcard { get; set; }

    public string? ExpenseCodeFleetcard { get; set; }

    public string? ExpenseUrlFleetcard { get; set; }

    public DateTime? ExpenseCreateDateFleetcard { get; set; }
}
