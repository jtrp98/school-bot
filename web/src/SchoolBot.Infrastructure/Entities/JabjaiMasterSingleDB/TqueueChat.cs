using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TqueueChat
{
    public int QueueChatId { get; set; }

    public string LineGroupId { get; set; } = null!;

    public int SchoolId { get; set; }

    public string Subject { get; set; } = null!;

    public string? Detail { get; set; }

    public int Priority { get; set; }

    public string Status { get; set; } = null!;

    public int? AssignStaffId { get; set; }

    public int QueuePosition { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public DateTime? AcceptDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? CsatScore { get; set; }

    public string? CsatFeedback { get; set; }

    public virtual TqueueAgent? AssignStaff { get; set; }

    public virtual ICollection<TqueueCsatSurvey> TqueueCsatSurveys { get; set; } = new List<TqueueCsatSurvey>();

    public virtual ICollection<TqueueLog> TqueueLogs { get; set; } = new List<TqueueLog>();
}
