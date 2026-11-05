using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class CollectionAlert
{
    public Guid AlertId { get; set; }

    public Guid CaseId { get; set; }

    public int SchoolId { get; set; }

    public string Type { get; set; } = null!;

    public string? Detail { get; set; }

    public int? AcknowledgedBy { get; set; }

    public DateTime? AcknowledgedAt { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual CollectionCase Case { get; set; } = null!;
}
