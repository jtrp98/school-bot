using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotSubmission
{
    public int SchoolBotSubmissionId { get; set; }

    public int LearnerId { get; set; }

    public int MissionId { get; set; }

    public int CriteriaId { get; set; }

    public int ProjectRevisionId { get; set; }

    public DateTime SubmittedAt { get; set; }

    public string State { get; set; } = null!;

    public int? CurrentAssessmentId { get; set; }

    public string? Evidence { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
