using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TplanCourseAssessmentTopic
{
    public int PlanCourseAssessmentTopicId { get; set; }

    public int? SchoolId { get; set; }

    public int? PlanCourseId { get; set; }

    public string? AssessmentName { get; set; }

    public string? AssessmentCategoryCode { get; set; }

    public int? Sequence { get; set; }

    public int? CourseTypeId { get; set; }

    public int? EventId { get; set; }

    public int? InsertedUserId { get; set; }

    public DateTime? InsertedDateTime { get; set; }

    public int? UpdatedUserId { get; set; }

    public DateTime? UpdatedDateTime { get; set; }

    public bool? IsDeleted { get; set; }

    public int? DeletedUserId { get; set; }

    public DateTime? DeletedDateTime { get; set; }
}
