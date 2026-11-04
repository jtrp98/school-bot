using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class Tcurriculum
{
    public int CurriculumId { get; set; }

    public string? CurriculumName { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public int? NYear { get; set; }

    public int SchoolId { get; set; }

    public bool? IsActive { get; set; }

    public bool CDel { get; set; }
}
