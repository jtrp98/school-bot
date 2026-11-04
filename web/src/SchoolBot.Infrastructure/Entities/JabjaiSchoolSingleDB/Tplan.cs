using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class Tplan
{
    public int PlanId { get; set; }

    public int CurriculumId { get; set; }

    public int NTsubLevel { get; set; }

    public string PlanName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public bool? IsActive { get; set; }

    public int SchoolId { get; set; }

    public bool CDel { get; set; }
}
