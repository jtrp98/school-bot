using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TstudentHealthInfo
{
    public int StudentHealthId { get; set; }

    public int NHealthId { get; set; }

    public int? SId { get; set; }

    public string? SBlood { get; set; }

    public string? SSickFood { get; set; }

    public string? SSickDrug { get; set; }

    public string? SSickOther { get; set; }

    public string? SSickNormal { get; set; }

    public string? SSickDanger { get; set; }

    public string? SDeleted { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public string? Notes { get; set; }
}
