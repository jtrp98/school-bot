using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tpage
{
    public int PageId { get; set; }

    public int? PageGroupId { get; set; }

    public string? PageName { get; set; }

    public string? PageUrl { get; set; }

    public bool? Active { get; set; }
}
