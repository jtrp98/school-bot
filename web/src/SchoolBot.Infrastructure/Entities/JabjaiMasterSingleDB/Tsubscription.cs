using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tsubscription
{
    public long SubscriptionId { get; set; }

    public string ParentCode { get; set; } = null!;

    public string SId { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? PurchaseBy { get; set; }

    public int SubscriptionPlanId { get; set; }

    public DateTime ExpireAt { get; set; }

    public bool RenewalPromptPending { get; set; }

    public string? Survey { get; set; }

    public string? SchoolId { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
