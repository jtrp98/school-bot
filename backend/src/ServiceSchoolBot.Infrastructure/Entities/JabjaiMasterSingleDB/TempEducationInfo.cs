using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TempEducationInfo
{
    public int SEmp { get; set; }

    public int Id { get; set; }

    public string? Institution { get; set; }

    public int? StudyYear { get; set; }

    public int? GraduationYear { get; set; }

    public int? LevelId { get; set; }

    public string? Major { get; set; }

    public string? MinorSubject { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool CDel { get; set; }
}
