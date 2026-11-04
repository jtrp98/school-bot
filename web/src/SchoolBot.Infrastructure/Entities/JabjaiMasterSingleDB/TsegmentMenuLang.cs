using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TsegmentMenuLang
{
    public Guid Id { get; set; }

    public string? SegmentMenuName { get; set; }

    public int? SegmentMenuId { get; set; }

    public string? Lang { get; set; }
}
