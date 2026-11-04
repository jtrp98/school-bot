using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotScoringPolicy
{
    public int SchoolBotScoringPolicyId { get; set; }

    public int CourseRunId { get; set; }

    public int Revision { get; set; }

    public string Policy { get; set; } = null!;

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
