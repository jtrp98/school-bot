using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtassessmentBackup
{
    public int? AssessmentId { get; set; }

    public string? AssessmentName { get; set; }

    public int? AssessmentTypeId { get; set; }

    public string? AssessmentMaxScore { get; set; }

    public string? NameIdentifier { get; set; }

    public string? MaxScoreIdentifier { get; set; }

    public int? SchoolId { get; set; }

    public int? NGradeId { get; set; }

    public int? NYear { get; set; }

    public string? NTerm { get; set; }

    public int? NTsubLevel { get; set; }

    public int? NTermSubLevel2 { get; set; }

    public int? SPlaneId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? SubmitPeriod { get; set; }

    public string? AssessmentNameEng { get; set; }

    public bool? IsActive { get; set; }

    public bool? CDel { get; set; }
}
