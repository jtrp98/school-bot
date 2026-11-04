using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class LessonPlanSubmission
{
    public int LessonPlanSubmissionId { get; set; }

    public int SchoolId { get; set; }

    public int LessonPlanId { get; set; }

    public int SubmissionNo { get; set; }

    public string Status { get; set; } = null!;

    public DateTime SubmittedAt { get; set; }

    public int? DecidedBy { get; set; }

    public DateTime? DecidedAt { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool Cdel { get; set; }

    public virtual LessonPlan LessonPlan { get; set; } = null!;

    public virtual ICollection<LessonPlanFile> LessonPlanFiles { get; set; } = new List<LessonPlanFile>();
}
