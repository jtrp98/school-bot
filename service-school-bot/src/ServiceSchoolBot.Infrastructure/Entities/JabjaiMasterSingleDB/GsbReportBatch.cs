using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class GsbReportBatch
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public string BatchReference { get; set; } = null!;

    public string Period { get; set; } = null!;

    public string SavingsType { get; set; } = null!;

    public DateTime CoversDate { get; set; }

    public int RecordCount { get; set; }

    public decimal Total { get; set; }

    public string StoragePath { get; set; } = null!;

    public DateTime GeneratedAt { get; set; }

    public int GeneratedByMakerUserId { get; set; }

    public string ReportType { get; set; } = null!;

    public virtual GsbMakerUser GeneratedByMakerUser { get; set; } = null!;

    public virtual ICollection<GsbSavingsDeposit> GsbSavingsDeposits { get; set; } = new List<GsbSavingsDeposit>();
}
