using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TsubcriptionPlan
{
    public int SubscriptionPlanId { get; set; }

    public string PlanName { get; set; } = null!;

    public string PlanCode { get; set; } = null!;

    public decimal Price { get; set; }

    public string CycleUnit { get; set; } = null!;

    public int CycleCount { get; set; }

    public bool IsActive { get; set; }

    public string? SchoolId { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
