using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TperiodTsubLevel
{
    public int NTsubLevel { get; set; }

    public string SPeriodId { get; set; } = null!;

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
