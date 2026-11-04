using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TstudentHistory
{
    public string StudentHistoryId { get; set; } = null!;

    public int? NTermSubLevel2Old { get; set; }

    public int? NTermSubLevel2New { get; set; }

    public string? DayStart { get; set; }

    public string? DayEnd { get; set; }

    public string? StudentStatus { get; set; }

    public DateTime? DayAdd { get; set; }

    public int? Useraddid { get; set; }

    public string? NTerm { get; set; }

    public int? SId { get; set; }

    public int SchoolId { get; set; }

    public DateTime? DProfessionalStandard { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? DiplomaCode { get; set; }

    public bool CDel { get; set; }
}
