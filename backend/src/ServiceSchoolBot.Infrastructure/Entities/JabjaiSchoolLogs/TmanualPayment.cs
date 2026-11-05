using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TmanualPayment
{
    public int Id { get; set; }

    public int TransactionId { get; set; }

    public string TransactionType { get; set; } = null!;

    public int ApproveId { get; set; }

    public string EvidenceUrl { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Remark { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? CancelerId { get; set; }
}
