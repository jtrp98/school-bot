using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class GsbMakerUser
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public string Username { get; set; } = null!;

    public string EncryptedPassword { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string ClassRoom { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDel { get; set; }

    public virtual ICollection<GsbAuditLogEntry> GsbAuditLogEntries { get; set; } = new List<GsbAuditLogEntry>();

    public virtual ICollection<GsbReportBatch> GsbReportBatches { get; set; } = new List<GsbReportBatch>();

    public virtual ICollection<GsbSavingsDeposit> GsbSavingsDeposits { get; set; } = new List<GsbSavingsDeposit>();
}
