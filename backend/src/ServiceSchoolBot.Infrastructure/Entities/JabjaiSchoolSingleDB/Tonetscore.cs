using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class Tonetscore
{
    public int Year { get; set; }

    public int SId { get; set; }

    public string Identification { get; set; } = null!;

    public int NTsubLevel { get; set; }

    public string? Class { get; set; }

    public string? SeatNo { get; set; }

    public decimal? SubjectScore61 { get; set; }

    public decimal? SubjectScore64 { get; set; }

    public decimal? SubjectScore65 { get; set; }

    public decimal? SubjectScore63 { get; set; }

    public decimal? SubjectScore91 { get; set; }

    public decimal? SubjectScore94 { get; set; }

    public decimal? SubjectScore95 { get; set; }

    public decimal? SubjectScore93 { get; set; }

    public decimal? SubjectScore01 { get; set; }

    public decimal? SubjectScore04 { get; set; }

    public decimal? SubjectScore05 { get; set; }

    public decimal? SubjectScore02 { get; set; }

    public decimal? SubjectScore03 { get; set; }

    public decimal? Result30Percent { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool CDel { get; set; }

    public decimal? ResultPercent { get; set; }

    public decimal? Multiplier { get; set; }
}
