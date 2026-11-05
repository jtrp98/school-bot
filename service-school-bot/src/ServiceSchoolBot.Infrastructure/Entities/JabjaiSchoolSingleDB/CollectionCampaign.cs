using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class CollectionCampaign
{
    public Guid CampaignId { get; set; }

    public int SchoolId { get; set; }

    public string Name { get; set; } = null!;

    public string Stage { get; set; } = null!;

    public string PlaybookId { get; set; } = null!;

    public string? TargetFilter { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? ScheduledAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual ICollection<CollectionCallAttempt> CollectionCallAttempts { get; set; } = new List<CollectionCallAttempt>();
}
