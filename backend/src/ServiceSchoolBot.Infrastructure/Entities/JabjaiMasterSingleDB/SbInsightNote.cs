using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class SbInsightNote
{
    public Guid Id { get; set; }

    public int? SchoolId { get; set; }

    public string Body { get; set; } = null!;

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual SbInsightSchoolSummary? School { get; set; }
}
