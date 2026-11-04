using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TposDevice
{
    public string PosId { get; set; } = null!;

    public int? TtabletId { get; set; }

    public int? TerminalId { get; set; }
}
