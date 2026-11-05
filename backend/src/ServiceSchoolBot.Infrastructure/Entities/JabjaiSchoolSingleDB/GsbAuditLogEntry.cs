using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class GsbAuditLogEntry
{
    public long Id { get; set; }

    public string EntityName { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public string Action { get; set; } = null!;

    public int PerformedByMakerUserId { get; set; }

    public DateTime PerformedAt { get; set; }

    public string? Details { get; set; }

    public virtual GsbMakerUser PerformedByMakerUser { get; set; } = null!;
}
