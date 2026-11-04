using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtgradeDetailBackup
{
    public int? NGradeDetailId { get; set; }

    public int? NGradeId { get; set; }

    public int? SId { get; set; }

    public string? ScoreMidTerm { get; set; }

    public string? ScoreFinalTerm { get; set; }

    public string? GetScore100 { get; set; }

    public string? GetGradeLabel { get; set; }

    public string? GetBehaviorLabel { get; set; }

    public string? GetReadWrite { get; set; }

    public string? GetSpecial { get; set; }

    public string? GetBehaviorTotal { get; set; }

    public string? GetQuiz100 { get; set; }

    public string? GetMid100 { get; set; }

    public string? GetFinal100 { get; set; }

    public string? GetSamattana { get; set; }

    public int? SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? CDel { get; set; }

    public string? ScoreBeforeAfterMidTerm { get; set; }

    public string? ScoreData { get; set; }

    public string? ScoreBeforeMidTerm { get; set; }

    public string? ScoreAfterMidTerm { get; set; }

    public string? GetReadWriteTotal { get; set; }

    public string? GetSamattanaTotal { get; set; }

    public string? GetBeforeQuiz100 { get; set; }

    public string? GetAfterQuiz100 { get; set; }

    public double? NCredit { get; set; }

    public double? CourseTotalHour { get; set; }

    public int? PlanId { get; set; }

    public string? GradeSymbol { get; set; }
}
