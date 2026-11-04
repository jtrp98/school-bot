using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TregisterRegularity
{
    public int RegisterRegularityId { get; set; }

    public int? NTsubLevel { get; set; }

    public string? Filename { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool CDel { get; set; }
}
