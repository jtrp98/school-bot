using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tattach
{
    public int AttachId { get; set; }

    public string? EventName { get; set; }

    public int? EventId { get; set; }

    public string? AttachUrl { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? AttachName { get; set; }

    public string? ContentType { get; set; }

    public DateTime? BacklogSyncedAt { get; set; }
}
