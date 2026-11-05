using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TsellDberror
{
    public DateTime? DSell { get; set; }

    public int? SId { get; set; }

    public int? SEmp { get; set; }

    public int? NSession { get; set; }

    public decimal? NTotal { get; set; }

    public int? SId2 { get; set; }

    public int? ShopId { get; set; }

    public DateTime? DayCancal { get; set; }

    public int? UserCancel { get; set; }

    public string? Id { get; set; }

    public int SSellId { get; set; }

    public string? ReferenceOrder { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public Guid? CardId { get; set; }

    public string? DeviceId { get; set; }

    public bool Online { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? Barcode { get; set; }

    public string? NfcorEncrypt { get; set; }

    public int? PaymentType { get; set; }

    public string? RefPromptPay { get; set; }

    public string? ApplicationName { get; set; }

    public string? SourceIp { get; set; }

    public bool Processed { get; set; }

    public string? ErrorInfo { get; set; }
}
