using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TsubcriptionInvoice
{
    public long SubscriptionInvoiceId { get; set; }

    public long? SubcriptionId { get; set; }

    public string Status { get; set; } = null!;

    public string? TransactionId { get; set; }

    public string? QrPayload { get; set; }

    public DateTime? QrExpireAt { get; set; }

    public string? SId { get; set; }

    public int? SubscriptionPlanId { get; set; }

    public string? ParentCode { get; set; }

    public string? SchoolId { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
