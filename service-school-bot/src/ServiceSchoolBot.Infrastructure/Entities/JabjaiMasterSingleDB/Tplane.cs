using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tplane
{
    public int SPlaneId { get; set; }

    public string? SPlaneName { get; set; }

    public char? CDel { get; set; }

    public double? NCredit { get; set; }

    public string? CourseCode { get; set; }

    public int? CourseType { get; set; }

    public double? CourseHour { get; set; }

    public double? CourseTotalHour { get; set; }

    public int? CourseGroup { get; set; }

    public string? NTsubLevel { get; set; }

    public int? NTerm { get; set; }

    public int? CourseStatus { get; set; }

    public int? SortNumber { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CourseCodeEn { get; set; }

    public string? CourseNameEn { get; set; }

    public int? SourceTypeCode { get; set; }

    public int? CurriculumType { get; set; }

    public bool? IsSystem { get; set; }
}
