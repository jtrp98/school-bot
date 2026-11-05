using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TempFamily
{
    public int SEmp { get; set; }

    public int Id { get; set; }

    public string? FamilyRelation { get; set; }

    public int? TitleId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTime? Birthday { get; set; }

    public int? PersonalStatus { get; set; }

    public string? LiveStatus { get; set; }

    public string? DeathStatus { get; set; }

    public string? FamilyCareer { get; set; }

    public int? LevelId { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool CDel { get; set; }

    public int? EducationBackground { get; set; }
}
