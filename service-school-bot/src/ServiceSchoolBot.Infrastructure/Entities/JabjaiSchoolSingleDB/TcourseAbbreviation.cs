using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TcourseAbbreviation
{
    public int CourseAbbreviationId { get; set; }

    public string ShortName { get; set; } = null!;

    public bool CDel { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int SPlaneId { get; set; }

    public int? SchoolId { get; set; }
}
