using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TsourceType
{
    public int SourceTypeCode { get; set; }

    public string SourceTypeName { get; set; } = null!;

    public string SystemCode { get; set; } = null!;

    public string? Description { get; set; }

    public string? Remark { get; set; }
}
