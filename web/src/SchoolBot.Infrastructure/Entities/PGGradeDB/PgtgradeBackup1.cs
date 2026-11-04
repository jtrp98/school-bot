using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtgradeBackup1
{
    public int? NGradeId { get; set; }

    public string? NTerm { get; set; }

    public int? SPlaneId { get; set; }

    public string? SNote { get; set; }

    public DateTime? DAdd { get; set; }

    public DateTime? DUpdate { get; set; }

    public int? NUserAdd { get; set; }

    public int? NUserUpdate { get; set; }

    public double? FRatioQuiz { get; set; }

    public double? FRatioMidTerm { get; set; }

    public double? FRatioLateTerm { get; set; }

    public string? MaxMidTerm { get; set; }

    public string? MaxFinalTerm { get; set; }

    public string? MaxGradeTotal { get; set; }

    public string? MaxBehaviorTotal { get; set; }

    public string? StudyMonday { get; set; }

    public string? StudyTuesday { get; set; }

    public string? StudyWednesday { get; set; }

    public string? StudyThursday { get; set; }

    public string? StudyFriday { get; set; }

    public string? StudySaturday { get; set; }

    public string? StudySunday { get; set; }

    public string? GradeDicimal { get; set; }

    public string? GradeAutoReadScore { get; set; }

    public string? GradeAutoBehaviorScore { get; set; }

    public string? GradeAddBehavior { get; set; }

    public string? GradeAddCheewat { get; set; }

    public string? GradeCloseBehaviorReadWrite { get; set; }

    public string? GradeShareData { get; set; }

    public int? NTermSubLevel2 { get; set; }

    public double? FRatioQuizPass { get; set; }

    public string? GradeCloseGrade { get; set; }

    public int? SortNumber { get; set; }

    public int? ReportPrint { get; set; }

    public string? GradeCloseSamattana { get; set; }

    public string? OptionMid { get; set; }

    public string? OptionFinal { get; set; }

    public int? SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? CDel { get; set; }

    public string? GradeAddSamattana { get; set; }

    public string? MaxReadWriteTotal { get; set; }

    public string? MaxSamattanaTotal { get; set; }

    public string? MaxBeforeMidTerm { get; set; }

    public string? MaxAfterMidTerm { get; set; }

    public bool? GradeShowFullScore { get; set; }

    public double? FRatioBeforeMidTerm { get; set; }

    public double? FRatioAfterMidTerm { get; set; }

    public int? PlanId { get; set; }

    public string? GradeDicimalForFinal { get; set; }
}
