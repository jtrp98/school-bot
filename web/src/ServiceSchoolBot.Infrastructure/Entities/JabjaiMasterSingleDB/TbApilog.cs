using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbApilog
{
    public int Id { get; set; }

    public string? Info { get; set; }

    public DateTime Tstamp { get; set; }
}
