using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TbStudentViewsV2
{
    public int? SId { get; set; }

    public string? SStudentTitle { get; set; }

    public string? SName { get; set; }

    public string? SLastname { get; set; }

    public string? NTerm { get; set; }

    public string? SubLevel { get; set; }

    public string? NTsubLevel2 { get; set; }

    public int? NTsubLevel { get; set; }

    public int? NTermSubLevel2 { get; set; }

    public string? SStudentId { get; set; }

    public int? NStudentNumber { get; set; }

    public int? NStudentStatus { get; set; }

    public char? CDel { get; set; }

    public string? STerm { get; set; }

    public int? NumberYear { get; set; }

    public string? TitleDescription { get; set; }

    public int? SchoolId { get; set; }

    public char? CSex { get; set; }

    public DateTime? DBirth { get; set; }

    public string? SNickName { get; set; }

    public string? SIdentification { get; set; }
}
