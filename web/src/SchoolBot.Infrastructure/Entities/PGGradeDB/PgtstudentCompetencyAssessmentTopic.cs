using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtstudentCompetencyAssessmentTopic
{
    public int StudentCompetencyAssessmentTopicId { get; set; }

    public int? StudentCompetencyAssessmentId { get; set; }

    public int? AssessmentTopicId { get; set; }

    public int? Sequence { get; set; }

    public int? SchoolId { get; set; }

    public int? EventId { get; set; }

    public int? InsertedUserId { get; set; }

    public DateTime? InsertedDateTime { get; set; }

    public bool? IsDeleted { get; set; }

    public int? DeletedUserId { get; set; }

    public DateTime? DeletedDateTime { get; set; }
}
