using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TsendGradePsi
{
    public int SchoolId { get; set; }

    public int NYear { get; set; }

    public int? Year { get; set; }

    public string NTerm { get; set; } = null!;

    public string? Term { get; set; }

    public int SId { get; set; }

    public int SPlaneId { get; set; }

    public string? CourseCode { get; set; }

    public string? Grade { get; set; }

    public DateTime? SendDate { get; set; }

    public int? StatusCode { get; set; }

    public string? ResponseContent { get; set; }
}
