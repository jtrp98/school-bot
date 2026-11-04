using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotCourseRunActivity
{
    public int SchoolBotCourseRunActivityId { get; set; }

    public int CourseRunId { get; set; }

    public int ActivityId { get; set; }

    public DateTime? OpensAt { get; set; }

    public DateTime? ClosesAt { get; set; }

    public string? PolicyOverride { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
