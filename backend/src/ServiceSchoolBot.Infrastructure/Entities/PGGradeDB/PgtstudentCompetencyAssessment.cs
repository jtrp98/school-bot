using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtstudentCompetencyAssessment
{
    public int StudentCompetencyAssessmentId { get; set; }

    public int? Nyear { get; set; }

    public string? Nterm { get; set; }

    public int? NtsubLevel { get; set; }

    public int? NtermSubLevel2 { get; set; }

    public int? SchoolId { get; set; }

    public int? SourceTypeCode { get; set; }

    public int? SubsourceTypeCode { get; set; }

    public int? InsertedUserId { get; set; }

    public DateTime? InsertedDateTime { get; set; }
}
