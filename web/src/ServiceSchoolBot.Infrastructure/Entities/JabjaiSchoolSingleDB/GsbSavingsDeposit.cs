using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class GsbSavingsDeposit
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public int StudentId { get; set; }

    public int RecordedByMakerUserId { get; set; }

    public string Period { get; set; } = null!;

    public string SavingsType { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime OccurredAt { get; set; }

    public string Reference { get; set; } = null!;

    public string IdempotencyKey { get; set; } = null!;

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ReportBatchId { get; set; }

    public int? CoibReportBatchId { get; set; }

    public virtual GsbMakerUser RecordedByMakerUser { get; set; } = null!;

    public virtual GsbReportBatch? ReportBatch { get; set; }
}
