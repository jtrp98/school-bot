using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TbStudentViewsV3
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

    public DateTime? DStart { get; set; }

    public DateTime? DEnd { get; set; }

    public int? NDeleted { get; set; }

    public int? NWorkingStatus { get; set; }

    public char? NTermSubLevel2Status { get; set; }

    public char? CSex { get; set; }

    public string? SStudentPicture { get; set; }

    public DateOnly? DayQuit { get; set; }

    public int? NTimeType { get; set; }

    public DateTime? MoveInDate { get; set; }
}
