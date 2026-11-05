using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class PromptPayTsellDetail
{
    public Guid Id { get; set; }

    public int SSellId { get; set; }

    public string TsellDetailData { get; set; } = null!;

    public bool Processed { get; set; }

    public DateTime Tstamp { get; set; }

    public int SchoolId { get; set; }
}
