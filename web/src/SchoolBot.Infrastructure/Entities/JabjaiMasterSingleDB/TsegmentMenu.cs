using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TsegmentMenu
{
    public int Id { get; set; }

    public int? GroupMenuId { get; set; }

    public string? Name { get; set; }

    public string? Class { get; set; }

    public string? Title { get; set; }

    public bool? Active { get; set; }

    public int? NOrder { get; set; }
}
