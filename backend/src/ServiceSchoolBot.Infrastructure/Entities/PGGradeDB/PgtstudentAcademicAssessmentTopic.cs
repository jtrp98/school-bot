using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtstudentAcademicAssessmentTopic
{
    public int StudentAcademicAssessmentTopicId { get; set; }

    public int? StudentAcademicAssessmentId { get; set; }

    public int? PlanCourseAssessmentTopicId { get; set; }

    public string? AssessmentCategoryCode { get; set; }

    public int? Sequence { get; set; }

    public int? SchoolId { get; set; }

    public int? EventId { get; set; }

    public int? InsertedUserId { get; set; }

    public DateTime? InsertedDateTime { get; set; }

    public bool? IsDeleted { get; set; }

    public int? DeletedUserId { get; set; }

    public DateTime? DeletedDateTime { get; set; }
}
