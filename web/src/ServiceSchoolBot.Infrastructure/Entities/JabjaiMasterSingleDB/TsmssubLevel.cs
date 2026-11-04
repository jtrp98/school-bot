using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TsmssubLevel
{
    public int NTsubLevel { get; set; }

    public int? NSms { get; set; }

    public int NSmssubId { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
