using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class CollectionCase
{
    public Guid CaseId { get; set; }

    public int SchoolId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string StudentName { get; set; } = null!;

    public string? GuardianName { get; set; }

    public string GuardianPhone { get; set; } = null!;

    public string? GuardianEmail { get; set; }

    public string AcademicYear { get; set; } = null!;

    public string Term { get; set; } = null!;

    public string? LevelName { get; set; }

    public string? ClassName { get; set; }

    public string? InvoiceNumber { get; set; }

    public decimal OutstandingAmount { get; set; }

    public DateTime? DueDate { get; set; }

    public string Stage { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? ExcludeReason { get; set; }

    public int? ExcludedBy { get; set; }

    public DateTime LastSyncedAt { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual ICollection<CollectionAlert> CollectionAlerts { get; set; } = new List<CollectionAlert>();

    public virtual ICollection<CollectionCallAttempt> CollectionCallAttempts { get; set; } = new List<CollectionCallAttempt>();

    public virtual ICollection<CollectionCsNote> CollectionCsNotes { get; set; } = new List<CollectionCsNote>();

    public virtual ICollection<CollectionFallbackMessage> CollectionFallbackMessages { get; set; } = new List<CollectionFallbackMessage>();

    public virtual ICollection<CollectionPromiseToPay> CollectionPromiseToPays { get; set; } = new List<CollectionPromiseToPay>();
}
