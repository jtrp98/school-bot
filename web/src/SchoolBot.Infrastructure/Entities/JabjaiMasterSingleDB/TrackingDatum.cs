using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TrackingDatum
{
    public int Id { get; set; }

    public int? Pid { get; set; }

    public long? ChVer { get; set; }

    public long? ChCrVer { get; set; }

    public string? ChOp { get; set; }

    public string? SPid { get; set; }
}
