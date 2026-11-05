using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TbCurrentTermDatum
{
    public string? NTerm { get; set; }

    public DateOnly? DStart { get; set; }

    public DateOnly? DEnd { get; set; }

    public int? SchoolId { get; set; }

    public int? NYear { get; set; }

    public string? STerm { get; set; }
}
