using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TregisterPlanSetup
{
    public int RegPlanId { get; set; }

    public int RegisterPlanSetupId { get; set; }

    public int NTsubLevel { get; set; }

    public string? PlanName { get; set; }

    public string? PlanCode { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool CDel { get; set; }

    public int? RegisterProgramSetupId { get; set; }
}
