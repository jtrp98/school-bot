using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TvoiceQueue
{
    public int VoiceQueueId { get; set; }

    public string? Phone { get; set; }

    public string? VoiceMessage { get; set; }

    public DateTime? ProcessDate { get; set; }

    public string? Status { get; set; }

    public string? Body { get; set; }

    public int? SchoolId { get; set; }

    public string? VoiceName { get; set; }

    public Guid? Session { get; set; }

    public string? CampaignId { get; set; }
}
