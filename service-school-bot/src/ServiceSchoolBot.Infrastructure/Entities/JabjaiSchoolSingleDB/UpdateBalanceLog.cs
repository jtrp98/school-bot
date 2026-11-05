using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class UpdateBalanceLog
{
    public int LogId { get; set; }

    public int SId { get; set; }

    public decimal OldBalance { get; set; }

    public decimal NewBalance { get; set; }

    public string Source { get; set; } = null!;

    public DateTime Tstatmp { get; set; }
}
