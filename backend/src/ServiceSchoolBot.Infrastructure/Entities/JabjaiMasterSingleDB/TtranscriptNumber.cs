using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TtranscriptNumber
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public string? NTerm { get; set; }

    public int NTsubLevel { get; set; }

    public int? NTermSubLevel2 { get; set; }

    public int SId { get; set; }

    public int? TranscriptSetNumber { get; set; }

    public int? TranscriptNumber { get; set; }

    public bool CDel { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
