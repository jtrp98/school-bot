using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class CompassStudentLevelScore
{
    public int SchoolId { get; set; }

    public int SId { get; set; }

    public string LevelCode { get; set; } = null!;

    public double OverallAvg { get; set; }

    public DateTime UpdatedAt { get; set; }
}
