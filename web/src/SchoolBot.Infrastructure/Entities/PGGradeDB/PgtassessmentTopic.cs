using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtassessmentTopic
{
    public int AssessmentTopicId { get; set; }

    public string? AssessmentType { get; set; }

    public int? Sequence { get; set; }

    public string? AssessmentName { get; set; }

    public string? AssessmentNameEng { get; set; }

    public int? AssessmentMaxScore { get; set; }

    public bool? IsActive { get; set; }
}
