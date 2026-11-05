using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TtermSubLevel2
{
    public int NTermSubLevel2 { get; set; }

    public string? NTerm { get; set; }

    public int NTsubLevel { get; set; }

    public string? NTsubLevel2 { get; set; }

    public char NTermSubLevel2Status { get; set; }

    public int? NTimeType { get; set; }

    public int? NWorkingStatus { get; set; }

    public int? NBranchSpecId { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
