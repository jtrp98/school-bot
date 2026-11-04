using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TpeakDatum
{
    public int PeakDataId { get; set; }

    public string? Name { get; set; }

    public string? Value { get; set; }

    public DateTime? UpdateDate { get; set; }
}
