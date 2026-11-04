using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class CollectionSchoolCredit
{
    public int SchoolId { get; set; }

    public decimal BalanceMinutes { get; set; }

    public decimal? LowBalanceThresholdMinutes { get; set; }

    public DateTime UpdateDate { get; set; }
}
