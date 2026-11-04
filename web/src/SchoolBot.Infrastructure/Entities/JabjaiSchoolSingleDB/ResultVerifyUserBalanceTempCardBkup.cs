using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class ResultVerifyUserBalanceTempCardBkup
{
    public Guid? CardHistoryId { get; set; }

    public decimal? Balance { get; set; }

    public double? SystemBalance { get; set; }

    public string? Sqlstatement { get; set; }

    public DateTime Tstamp { get; set; }

    public DateTime? BusinessDate { get; set; }
}
