using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tconnect
{
    public int NConnectId { get; set; }

    public string? SMacPc { get; set; }

    public string? SMacMoblie { get; set; }

    public string? CStatus { get; set; }

    public string? SValue { get; set; }

    public string? SDisplay { get; set; }

    public string? CTypeConnect { get; set; }

    public string? SList { get; set; }

    public DateTime? DList { get; set; }

    public DateTime? DUpdate { get; set; }

    public int? CustomerId { get; set; }

    public int? EmployessId { get; set; }
}
