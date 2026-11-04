using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TschoolSummary
{
    public int SchoolId { get; set; }

    public int? StudentCount { get; set; }

    public string? CurrentTermId { get; set; }

    public int? CurrentYearId { get; set; }

    public DateTime? Modified { get; set; }
}
