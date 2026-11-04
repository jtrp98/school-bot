using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotAuditEvent
{
    public int SchoolBotAuditEventId { get; set; }

    public string Actor { get; set; } = null!;

    public string Event { get; set; } = null!;

    public string EntityType { get; set; } = null!;

    public int EntityId { get; set; }

    public Guid RequestId { get; set; }

    public string Reason { get; set; } = null!;

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? SchoolId { get; set; }
}
