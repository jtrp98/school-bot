using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TsettingExtraTime
{
    public int NSettingExtraTime { get; set; }

    public int SEmp { get; set; }

    public int NTermSubLevel2 { get; set; }

    public string NTerm { get; set; } = null!;

    public DateTime AddDate { get; set; }

    public int SPlaneId { get; set; }

    public int? UseToken { get; set; }

    public int? CDel { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
