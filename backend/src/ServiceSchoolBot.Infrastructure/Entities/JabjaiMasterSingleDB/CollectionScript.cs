using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class CollectionScript
{
    public Guid ScriptId { get; set; }

    public int SchoolId { get; set; }

    public string Stage { get; set; } = null!;

    public int Version { get; set; }

    public string Body { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreateDate { get; set; }
}
