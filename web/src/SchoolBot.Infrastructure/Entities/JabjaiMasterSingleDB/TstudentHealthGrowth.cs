using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TstudentHealthGrowth
{
    public int NHealthId { get; set; }

    public int NTsubLevel { get; set; }

    public int NMonth { get; set; }

    public decimal? Weight { get; set; }

    public decimal? Height { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
