using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TsweepLog
{
    public int SweepLogId { get; set; }

    public int TransType { get; set; }

    public int TransId { get; set; }

    public string? ChargeId { get; set; }

    public string ReferenceNo { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public int UpdateBy { get; set; }

    public string Username { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public int SchoolId { get; set; }

    public int SweepLogUserType { get; set; }

    public string Bank { get; set; } = null!;

    public int UserId { get; set; }

    public string UpdateName { get; set; } = null!;
}
