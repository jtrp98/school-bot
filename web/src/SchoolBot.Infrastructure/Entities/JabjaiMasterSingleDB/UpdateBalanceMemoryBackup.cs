using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class UpdateBalanceMemoryBackup
{
    public int LogId { get; set; }

    public int? SId { get; set; }

    public int? SEmp { get; set; }

    public int? SchoolId { get; set; }

    public decimal NMoney { get; set; }

    public DateTime Tstamp { get; set; }

    public string? Source { get; set; }
}
