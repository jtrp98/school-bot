using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tcalllog
{
    public string Id { get; set; } = null!;

    public string TenantId { get; set; } = null!;

    public string? CampaignId { get; set; }

    public string? CampaignName { get; set; }

    public string? ContactName { get; set; }

    public string? FromPhoneNumber { get; set; }

    public string? ToPhoneNumber { get; set; }

    public string? Direction { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public int? DurationSeconds { get; set; }

    public string? EndReason { get; set; }

    public string? AttemptStatus { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
