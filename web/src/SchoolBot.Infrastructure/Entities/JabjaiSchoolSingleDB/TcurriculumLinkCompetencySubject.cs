using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TcurriculumLinkCompetencySubject
{
    public int CurriculumLinkCompetencySubjectId { get; set; }

    public int? CurriculumLinkId { get; set; }

    public int? CompetencySplaneId { get; set; }

    public decimal? StudyWeightPercent { get; set; }

    public decimal? FinalWeightPercent { get; set; }

    public int? StudyMinute { get; set; }

    public int? Sequence { get; set; }

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
