using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class SchoolDbtemployee
{
    public int? SEmp { get; set; }

    public string? SIdentification { get; set; }

    public int? SchoolId { get; set; }

    public string? CType { get; set; }

    public decimal? NMoney { get; set; }

    public string? SName { get; set; }

    public string? SLastname { get; set; }

    public string? SPicture { get; set; }

    public char? CDel { get; set; }

    public decimal? NMax { get; set; }

    public decimal? DailyUsageLimit { get; set; }
}
