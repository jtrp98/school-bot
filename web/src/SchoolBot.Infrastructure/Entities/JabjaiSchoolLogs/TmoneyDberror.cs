using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TmoneyDberror
{
    public int MoneyId { get; set; }

    public DateTime DMoney { get; set; }

    public int SId { get; set; }

    public decimal? NMoney { get; set; }

    public char? CType { get; set; }

    public int? SEmp { get; set; }

    public decimal? NBalance { get; set; }

    public string? TopupType { get; set; }

    public DateTime? DayCancal { get; set; }

    public int? UserCancel { get; set; }

    public string TopupId { get; set; } = null!;

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public Guid? CardHistoryId { get; set; }

    public Guid ReferenceId { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public decimal Insurance { get; set; }

    public string? ChargeId { get; set; }

    public string? DeviceId { get; set; }

    public string? PaymentInfo { get; set; }

    public string? SourceIp { get; set; }

    public string? CheckId { get; set; }

    public bool Processed { get; set; }

    public string? ErrorDescription { get; set; }
}
