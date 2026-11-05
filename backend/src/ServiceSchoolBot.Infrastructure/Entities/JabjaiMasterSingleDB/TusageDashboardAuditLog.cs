using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TusageDashboardAuditLog
{
    public int Id { get; set; }

    public string Action { get; set; } = null!;

    public string? Detail { get; set; }

    public int AdminId { get; set; }

    public string AdminName { get; set; } = null!;

    public string? Ip { get; set; }

    public DateTime CreatedAt { get; set; }
}
