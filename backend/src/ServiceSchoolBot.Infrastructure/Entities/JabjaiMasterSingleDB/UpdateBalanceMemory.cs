using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class UpdateBalanceMemory
{
    public int LogId { get; set; }

    public int? SId { get; set; }

    public int? SEmp { get; set; }

    public int? SchoolId { get; set; }

    public decimal NMoney { get; set; }

    public DateTime Tstamp { get; set; }

    public string? Source { get; set; }
}
