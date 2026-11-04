using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TqueueCsatSurvey
{
    public int QueueCsatSurveyId { get; set; }

    public int QueueChatId { get; set; }

    public int Score { get; set; }

    public string? Feedback { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual TqueueChat QueueChat { get; set; } = null!;
}
