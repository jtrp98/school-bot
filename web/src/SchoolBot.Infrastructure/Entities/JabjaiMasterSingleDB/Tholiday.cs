using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tholiday
{
    public string NHoliday { get; set; } = null!;

    public int? SEmp { get; set; }

    public string? SHoliday { get; set; }

    public string? SHolidayEn { get; set; }

    public short? TimeType { get; set; }

    public char? SHolidayType { get; set; }

    public DateTime? DHolidayStart { get; set; }

    public DateTime? DHolidayEnd { get; set; }

    public char? CDel { get; set; }

    public char? SHolidayAll { get; set; }

    public string? SWhoSeeThis { get; set; }

    public int? SColor { get; set; }

    public string? SGroupTarget { get; set; }

    public bool? CStatusActive { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
