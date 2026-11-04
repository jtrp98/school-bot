using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbStudentViewsFiltered
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

    public DateTime? MoveInDate { get; set; }

    public int? NTimeType { get; set; }

    public string? ContactPeak { get; set; }

    public int? NTlevel { get; set; }

    public DateTime? MoveOutDate { get; set; }

    public string? ClassFullName { get; set; }

    public int? NYear { get; set; }

    public string? ClassFullNameEng { get; set; }

    public string? SStudentNameEn { get; set; }

    public string? SStudentLastEn { get; set; }
}
