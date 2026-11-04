using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TsellApi
{
    public long Id { get; set; }

    public string? SalesInfo { get; set; }

    public string? ProductInfo { get; set; }

    public DateTime? Tstamp { get; set; }
}
