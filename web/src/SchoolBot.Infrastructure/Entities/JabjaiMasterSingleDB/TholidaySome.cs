using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TholidaySome
{
    public string NHolidaySomeId { get; set; } = null!;

    public string? NHoliday { get; set; }

    public int? NTsubLevel { get; set; }

    public int? NTimeType { get; set; }

    public int? Deleted { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
