using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class UserAuditTable
{
    public int AuditId { get; set; }

    public string TableName { get; set; } = null!;

    public string OperationType { get; set; } = null!;

    public DateTime? Tstamp { get; set; }

    public int? OldId { get; set; }

    public int? NewId { get; set; }

    public string? StringId { get; set; }
}
