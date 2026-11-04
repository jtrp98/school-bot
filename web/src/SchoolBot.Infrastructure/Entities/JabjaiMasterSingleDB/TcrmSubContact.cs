using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcrmSubContact
{
    public int CrmSubContactId { get; set; }

    public int? CrmContactId { get; set; }

    public string? Position { get; set; }

    public string? Name { get; set; }

    public string? Tel { get; set; }

    public string? Email { get; set; }
}
