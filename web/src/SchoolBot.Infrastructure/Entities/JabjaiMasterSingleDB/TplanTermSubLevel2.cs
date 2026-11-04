using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TplanTermSubLevel2
{
    public int PlanTermSubLevel2Id { get; set; }

    public int PlanId { get; set; }

    public int NTermSubLevel2 { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public bool? IsActive { get; set; }

    public int SchoolId { get; set; }

    public bool CDel { get; set; }
}
