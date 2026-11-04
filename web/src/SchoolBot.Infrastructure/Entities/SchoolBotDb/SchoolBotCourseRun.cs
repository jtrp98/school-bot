using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotCourseRun
{
    public int SchoolBotCourseRunId { get; set; }

    public int CurriculumId { get; set; }

    public int ClassroomId { get; set; }

    public int Nyear { get; set; }

    public string Nterm { get; set; } = null!;

    public List<int> PlanCourseIds { get; set; } = null!;

    public int? CurrentPolicyId { get; set; }

    public int? PreviousRunId { get; set; }

    public DateTime EffectiveAt { get; set; }

    public string Lifecycle { get; set; } = null!;

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
