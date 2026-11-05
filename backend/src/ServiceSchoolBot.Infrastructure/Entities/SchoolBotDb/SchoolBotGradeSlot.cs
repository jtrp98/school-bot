using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotGradeSlot
{
    public int SchoolBotGradeSlotId { get; set; }

    public int CourseRunId { get; set; }

    public int PlanCourseId { get; set; }

    public int CourseId { get; set; }

    public int NgradeId { get; set; }

    public int AssessmentId { get; set; }

    public string ScoreIdentifier { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string TargetSnapshot { get; set; } = null!;

    public string OwnershipReason { get; set; } = null!;

    public DateTime? RetiredAt { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
