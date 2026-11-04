using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotRetakeGrant
{
    public int SchoolBotRetakeGrantId { get; set; }

    public int CourseRunId { get; set; }

    public int ActivityId { get; set; }

    public int LearnerId { get; set; }

    public string Reason { get; set; } = null!;

    public int? ConsumedAttemptId { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
