using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class CompassLevelSubjectStat
{
    public string LevelCode { get; set; } = null!;

    public string? LevelName { get; set; }

    public string Subject { get; set; } = null!;

    public double MeanScore { get; set; }

    public int StudentCount { get; set; }

    public int SchoolCount { get; set; }

    public DateTime UpdatedAt { get; set; }
}
