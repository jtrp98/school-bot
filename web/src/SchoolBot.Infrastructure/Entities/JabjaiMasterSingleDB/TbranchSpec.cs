using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbranchSpec
{
    public int BranchSpecId { get; set; }

    public int BranchSubjectId { get; set; }

    public string? BranchSpecName { get; set; }

    public int? CDel { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
