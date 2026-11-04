using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TgroupMenu
{
    public int Groupmenuid { get; set; }

    public string? Groupmenu { get; set; }

    public string? Class { get; set; }

    public string? Title { get; set; }

    public bool? Active { get; set; }

    public int? GroupOrder { get; set; }

    public bool? Actvice { get; set; }

    public int? NewOrder { get; set; }

    public string? Icon { get; set; }

    public bool? IsDev { get; set; }
}
