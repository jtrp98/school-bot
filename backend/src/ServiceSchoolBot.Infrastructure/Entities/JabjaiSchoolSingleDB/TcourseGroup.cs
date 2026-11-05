using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TcourseGroup
{
    public int CourseGroupId { get; set; }

    public string? Description { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public string? MasterCode { get; set; }

    public string? DescriptionEn { get; set; }

    public int? PurposeCode { get; set; }
}
