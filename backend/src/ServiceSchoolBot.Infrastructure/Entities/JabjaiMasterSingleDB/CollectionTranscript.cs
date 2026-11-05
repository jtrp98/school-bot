using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class CollectionTranscript
{
    public Guid TranscriptId { get; set; }

    public Guid AttemptId { get; set; }

    public int Seq { get; set; }

    public string Speaker { get; set; } = null!;

    public string Text { get; set; } = null!;

    public decimal? StartedAtSec { get; set; }

    public decimal? EndedAtSec { get; set; }

    public virtual CollectionCallAttempt Attempt { get; set; } = null!;
}
