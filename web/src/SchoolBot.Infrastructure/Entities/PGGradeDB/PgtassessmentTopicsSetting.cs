using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtassessmentTopicsSetting
{
    public int SettingId { get; set; }

    public int Nyear { get; set; }

    public string? AssessmentType { get; set; }

    public string AssessmentName { get; set; } = null!;

    public string AssessmentNameEng { get; set; } = null!;

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public int SchoolId { get; set; }

    public string? AssessmentMaxScore { get; set; }
}
