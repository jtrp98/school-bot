using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtstudentAcademicAssessment
{
    public int StudentAcademicAssessmentId { get; set; }

    public int? Nyear { get; set; }

    public string? Nterm { get; set; }

    public int? NtsubLevel { get; set; }

    public int? NtermSubLevel2 { get; set; }

    public int? SplaneId { get; set; }

    public decimal? ContinuousAssessmentMaxScore { get; set; }

    public decimal? FinalTermMaxScore { get; set; }

    public decimal? TotalMaxScore { get; set; }

    public int? SchoolId { get; set; }

    public int? EventId { get; set; }

    public int? SourceTypeCode { get; set; }

    public int? SubsourceTypeCode { get; set; }

    public int? InsertedUserId { get; set; }

    public DateTime? InsertedDateTime { get; set; }

    public int? UpdatedUserId { get; set; }

    public DateTime? UpdatedDateTime { get; set; }
}
