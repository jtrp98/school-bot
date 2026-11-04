using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TmenuPermission
{
    public int Id { get; set; }

    public int? MenuId { get; set; }

    public string? MenuName { get; set; }

    public string? MenuUrl { get; set; }
}
