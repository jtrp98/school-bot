using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TmenuLang
{
    public Guid Id { get; set; }

    public string? MenuName { get; set; }

    public int? MenuId { get; set; }

    public string? Lang { get; set; }
}
