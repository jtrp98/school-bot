using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class LessonPlan
{
    public int LessonPlanId { get; set; }

    public int SchoolId { get; set; }

    public int EmpId { get; set; }

    public int Nyear { get; set; }

    public string Nterm { get; set; } = null!;

    public int SplaneId { get; set; }

    public int NtsubLevel { get; set; }

    public string Status { get; set; } = null!;

    public int LatestSubmissionNo { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool Cdel { get; set; }

    public virtual ICollection<LessonPlanSubmission> LessonPlanSubmissions { get; set; } = new List<LessonPlanSubmission>();
}
