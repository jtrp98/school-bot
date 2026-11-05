using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtstudentAcademicAssessmentScore
{
    public int StudentAcademicAssessmentScoreId { get; set; }

    public int? StudentAcademicAssessmentId { get; set; }

    public int? Sid { get; set; }

    public decimal? ContinuousAssessmentScore { get; set; }

    public decimal? FinalTermScore { get; set; }

    public decimal? TotalScore { get; set; }

    public string? ScoreData { get; set; }

    public int? SchoolId { get; set; }

    public int? EventId { get; set; }

    public int? InsertedUserId { get; set; }

    public DateTime? InsertedDateTime { get; set; }

    public int? UpdatedUserId { get; set; }

    public DateTime? UpdatedDateTime { get; set; }

    public bool? IsDeleted { get; set; }

    public int? DeletedUserId { get; set; }

    public DateTime? DeletedDateTime { get; set; }
}
