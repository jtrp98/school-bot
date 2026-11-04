using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotAttempt
{
    public int SchoolBotAttemptId { get; set; }

    public int CourseRunId { get; set; }

    public int ActivityId { get; set; }

    public int LearnerId { get; set; }

    public int Sequence { get; set; }

    public string State { get; set; } = null!;

    public DateTime StartedAt { get; set; }

    public DateTime? DeadlineAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public string Snapshot { get; set; } = null!;

    public string Answers { get; set; } = null!;

    public decimal? EarnedScore { get; set; }

    public decimal MaxScore { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
