using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Ttoken
{
    public int TokenId { get; set; }

    public string? ClientToken { get; set; }

    public string? UserToken { get; set; }

    public int? SchoolId { get; set; }

    public DateTime? ClientTimeStamp { get; set; }

    public string? JabJaiToken { get; set; }
}
