using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TjobList
{
    public int NSchoolId { get; set; }

    public string? JobDescription { get; set; }

    public string? Deleted { get; set; }

    public string? WorkStatus { get; set; }

    public int NJobid { get; set; }

    public char? EmpType { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
