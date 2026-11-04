using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class CompassMbtiResult
{
    public int SchoolId { get; set; }

    public int SId { get; set; }

    public string PathCode { get; set; } = null!;

    public string? CareerName { get; set; }

    public int? MatchPercent { get; set; }

    public string? Reason { get; set; }

    public string? Answers { get; set; }

    public DateTime CreatedAt { get; set; }
}
