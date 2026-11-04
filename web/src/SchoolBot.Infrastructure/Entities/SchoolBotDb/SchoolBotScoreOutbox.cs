using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotScoreOutbox
{
    public int SchoolBotScoreOutboxId { get; set; }

    public int ScoreExportId { get; set; }

    public int ScorePreviewItemId { get; set; }

    public Guid OperationKey { get; set; }

    public string PayloadHash { get; set; } = null!;

    public string State { get; set; } = null!;

    public int Attempts { get; set; }

    public string? LeaseOwner { get; set; }

    public DateTime? LeaseUntil { get; set; }

    public DateTime NextAttemptAt { get; set; }

    public long? ReceiptId { get; set; }

    public string? ReceiptHash { get; set; }

    public string? LastErrorCode { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
