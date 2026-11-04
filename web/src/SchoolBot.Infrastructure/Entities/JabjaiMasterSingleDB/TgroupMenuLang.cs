using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TgroupMenuLang
{
    public Guid Id { get; set; }

    public string? GroupMenuName { get; set; }

    public int? Groupmenuid { get; set; }

    public string? Lang { get; set; }
}
