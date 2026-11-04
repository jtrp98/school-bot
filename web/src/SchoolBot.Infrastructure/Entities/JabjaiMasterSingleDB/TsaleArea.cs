using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TsaleArea
{
    public int SaleAreaId { get; set; }

    public string? Name { get; set; }

    public string? Text { get; set; }

    public string? Mapping { get; set; }

    public string? Provinces { get; set; }
}
