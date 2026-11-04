using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtgradeEditsHistory
{
    public int GradeEditsId { get; set; }

    public int GradeDetailId { get; set; }

    public decimal? GradeCalculation { get; set; }

    public decimal? GradeSet { get; set; }

    public int? UserUpdate { get; set; }

    public DateTime? DateUpdate { get; set; }

    public int? UseGradeSet { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public string? ScoreMidTerm { get; set; }

    public string? ScoreFinalTerm { get; set; }

    public string? GetScore100 { get; set; }

    public string? GetMid100 { get; set; }

    public string? GetFinal100 { get; set; }

    public string? GetGradeLabel { get; set; }
}
