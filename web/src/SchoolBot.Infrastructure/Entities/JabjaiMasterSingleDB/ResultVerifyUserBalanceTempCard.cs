using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class ResultVerifyUserBalanceTempCard
{
    public Guid? CardHistoryId { get; set; }

    public decimal? Balance { get; set; }

    public double? SystemBalance { get; set; }

    public string? Sqlstatement { get; set; }

    public DateTime? Tstamp { get; set; }
}
