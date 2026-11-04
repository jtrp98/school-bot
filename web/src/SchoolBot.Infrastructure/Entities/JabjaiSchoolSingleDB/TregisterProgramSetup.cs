using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TregisterProgramSetup
{
    public int RegProgramId { get; set; }

    public int RegisterProgramSetupId { get; set; }

    public int NTsubLevel { get; set; }

    public int SchoolId { get; set; }

    public string? ProgramName { get; set; }

    public string? ProgramCode { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public bool CDel { get; set; }
}
