using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TsendDataPsi
{
    public int SchoolId { get; set; }

    public int NYear { get; set; }

    public int SId { get; set; }

    public DateTime? SendDate { get; set; }

    public int? StatusCode { get; set; }

    public string? ResponseContent { get; set; }

    public DateTime? SendDate2 { get; set; }

    public int? StatusCode2 { get; set; }

    public string? ResponseContent2 { get; set; }
}
