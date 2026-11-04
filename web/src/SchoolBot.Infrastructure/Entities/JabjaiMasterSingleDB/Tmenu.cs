using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tmenu
{
    public int MenuId { get; set; }

    public string? MenuName { get; set; }

    public string? Url { get; set; }

    public string? Title { get; set; }

    public string? UrlDev { get; set; }

    public string? Class { get; set; }

    public string? MenuMode { get; set; }

    public int? MenuType { get; set; }

    public int? MenuIndex { get; set; }

    public int? Groupmenuid { get; set; }

    public int? NMenuOrder { get; set; }

    public bool? Showmenu { get; set; }

    public bool? Demo { get; set; }

    public int? Target { get; set; }

    public int? Submenu { get; set; }

    public bool? Active { get; set; }

    public bool? Actvice { get; set; }

    public int? SegmentId { get; set; }

    public int? NMenuOrder2 { get; set; }

    public bool? IsExceptAuth { get; set; }
}
