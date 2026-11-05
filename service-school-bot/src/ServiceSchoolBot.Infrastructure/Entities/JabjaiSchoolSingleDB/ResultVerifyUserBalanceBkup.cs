using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class ResultVerifyUserBalanceBkup
{
    public int SchoolId { get; set; }

    public int SId { get; set; }

    public decimal? Balance { get; set; }

    public double SystemBalance { get; set; }

    public string? Sqlstatement { get; set; }

    public DateTime Tstamp { get; set; }

    public DateTime? BusinessDate { get; set; }

    public string? CType { get; set; }
}
