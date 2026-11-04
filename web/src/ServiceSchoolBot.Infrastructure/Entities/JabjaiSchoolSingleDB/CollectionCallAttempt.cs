using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class CollectionCallAttempt
{
    public Guid AttemptId { get; set; }

    public Guid CaseId { get; set; }

    public Guid? CampaignId { get; set; }

    public int AttemptNo { get; set; }

    public string Stage { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }

    public DateTime? CalledAt { get; set; }

    public int? DurationSec { get; set; }

    public string? Outcome { get; set; }

    public string? ResultIntent { get; set; }

    public string? AladinCallId { get; set; }

    public string? RecordingUrl { get; set; }

    public string? RawWebhook { get; set; }

    public DateTime CreateDate { get; set; }

    public string? BillableEvent { get; set; }

    public decimal? BilledAmount { get; set; }

    public decimal? RateApplied { get; set; }

    public decimal? CreditBalanceAfter { get; set; }

    public decimal? MinutesBilled { get; set; }

    public virtual CollectionCampaign? Campaign { get; set; }

    public virtual CollectionCase Case { get; set; } = null!;

    public virtual ICollection<CollectionTranscriptAccessLog> CollectionTranscriptAccessLogs { get; set; } = new List<CollectionTranscriptAccessLog>();

    public virtual ICollection<CollectionTranscript> CollectionTranscripts { get; set; } = new List<CollectionTranscript>();
}
