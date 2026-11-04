using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class SchoolDbtuser
{
    public int? SId { get; set; }

    public string? SIdentification { get; set; }

    public int? SchoolId { get; set; }

    public char? CType { get; set; }

    public decimal? NMoney { get; set; }

    public string? SName { get; set; }

    public string? SLastname { get; set; }

    public string? SStudentPicture { get; set; }

    public int? NStudentStatus { get; set; }

    public char? CDel { get; set; }

    public decimal? NMax { get; set; }

    public string? SStudentId { get; set; }

    public decimal? DailyUsageLimit { get; set; }
}
