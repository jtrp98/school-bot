using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotAssessment
{
    public int SchoolBotAssessmentId { get; set; }

    public int SubmissionId { get; set; }

    public int Sequence { get; set; }

    public string Method { get; set; } = null!;

    public string Outcome { get; set; } = null!;

    public decimal? EarnedScore { get; set; }

    public decimal? MaxScore { get; set; }

    public string RuleResults { get; set; } = null!;

    public string Feedback { get; set; } = null!;

    public string? GraderVersion { get; set; }

    public int? AssessorId { get; set; }

    public int? SupersedesId { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
