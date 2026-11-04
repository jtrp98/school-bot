using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TsubInsurance
{
    public int SubInsuranceId { get; set; }

    public int? InsuranceId { get; set; }

    public string? ProductName { get; set; }

    public int? Qty { get; set; }

    public string? SerialNo { get; set; }

    public string? InsuranceNo { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
