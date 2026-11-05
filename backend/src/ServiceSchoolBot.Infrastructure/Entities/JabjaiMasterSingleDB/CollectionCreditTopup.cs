using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class CollectionCreditTopup
{
    public Guid TopupId { get; set; }

    public int SchoolId { get; set; }

    public decimal MinutesPurchased { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime EffectiveDate { get; set; }

    public string? Note { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreateDate { get; set; }

    public string? PeakInvoiceId { get; set; }

    public string? PeakInvoiceNo { get; set; }

    public string? PeakDocumentUrl { get; set; }

    public string PeakSyncStatus { get; set; } = null!;
}
