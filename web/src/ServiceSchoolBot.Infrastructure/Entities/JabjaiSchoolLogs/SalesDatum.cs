using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class SalesDatum
{
    public long Id { get; set; }

    public string? Salesinfo { get; set; }

    public string? Productinfo { get; set; }

    public DateTime? Tstamp { get; set; }
}
