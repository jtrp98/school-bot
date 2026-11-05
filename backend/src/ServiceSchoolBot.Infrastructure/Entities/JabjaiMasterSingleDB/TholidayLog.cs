using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TholidayLog
{
    public int LogId { get; set; }

    public int? SchoolId { get; set; }

    public string? Detail { get; set; }

    public int? Creator { get; set; }

    public DateTime? Created { get; set; }

    public string? HolidayId { get; set; }
}
