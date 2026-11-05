using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TsoundStudent
{
    public int SchoolId { get; set; }

    public int SId { get; set; }

    public int NTermSubLevel2 { get; set; }

    public string? FullName { get; set; }

    public string? Code { get; set; }

    public string? NickName { get; set; }

    public string? Receiver { get; set; }

    public string? Base64Sound { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }

    public bool? IsDel { get; set; }

    public string? Gate { get; set; }

    public string? TermId { get; set; }

    public short Soundtype { get; set; }
}
