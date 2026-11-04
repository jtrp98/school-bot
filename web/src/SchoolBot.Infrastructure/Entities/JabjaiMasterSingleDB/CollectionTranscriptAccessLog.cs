using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class CollectionTranscriptAccessLog
{
    public Guid LogId { get; set; }

    public Guid AttemptId { get; set; }

    public int AdminId { get; set; }

    public DateTime AccessedAt { get; set; }

    public virtual CollectionCallAttempt Attempt { get; set; } = null!;
}
