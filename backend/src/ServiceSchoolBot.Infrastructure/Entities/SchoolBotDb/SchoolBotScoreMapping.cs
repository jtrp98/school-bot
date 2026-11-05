using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotScoreMapping
{
    public int SchoolBotScoreMappingId { get; set; }

    public int CourseRunId { get; set; }

    public int GradeSlotId { get; set; }

    public string Sources { get; set; } = null!;

    public DateTime? RetiredAt { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
