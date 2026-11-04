using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotGradingJob
{
    public int SchoolBotGradingJobId { get; set; }

    public int SubmissionId { get; set; }

    public string State { get; set; } = null!;

    public int Attempts { get; set; }

    public string? LeaseOwner { get; set; }

    public DateTime? LeaseUntil { get; set; }

    public DateTime NextAttemptAt { get; set; }

    public string? LastErrorCode { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
