using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class CollectionSetting
{
    public int SchoolId { get; set; }

    public string CallWindowStart { get; set; } = null!;

    public string CallWindowEnd { get; set; } = null!;

    public bool CallOnWeekend { get; set; }

    public int MaxRetry { get; set; }

    public int RetryGapHours { get; set; }

    public string FallbackChannels { get; set; } = null!;

    public string? AladinPlaybookMap { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime UpdateDate { get; set; }
}
