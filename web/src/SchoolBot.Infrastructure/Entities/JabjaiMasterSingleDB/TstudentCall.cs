using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TstudentCall
{
    public DateTime CallDate { get; set; }

    public int SId { get; set; }

    public int SchoolId { get; set; }

    public short? CardNo { get; set; }

    public short? Status { get; set; }

    public TimeOnly? Created { get; set; }

    public TimeOnly? Announced { get; set; }

    public TimeOnly? Completed { get; set; }

    public bool? IsResend { get; set; }

    public string? ScanType { get; set; }

    public Guid? Token { get; set; }

    public string? Remark { get; set; }

    public string? GateToken { get; set; }
}
