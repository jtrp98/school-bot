using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TgradeShareInfo
{
    public int NGradeShareInfoId { get; set; }

    public string NTerm { get; set; } = null!;

    public int SPlaneId { get; set; }

    public int FromNTsubLevel2 { get; set; }

    public int ToNTsubLevel2 { get; set; }

    public int? CDel { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
