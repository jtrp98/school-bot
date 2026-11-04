using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TtitleList
{
    public int NSchoolId { get; set; }

    public string? TitleDescription { get; set; }

    public string? TitleDescriptionEn { get; set; }

    public string? Deleted { get; set; }

    public string? WorkStatus { get; set; }

    public int NTitleid { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public string? MasterCode { get; set; }

    public string? NTitleIdStr { get; set; }
}
