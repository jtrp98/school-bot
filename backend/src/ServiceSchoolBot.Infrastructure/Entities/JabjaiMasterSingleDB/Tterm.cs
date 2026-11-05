using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tterm
{
    public string NTerm { get; set; } = null!;

    public int? NYear { get; set; }

    public int? NumberTerm { get; set; }

    public char? TermStatus { get; set; }

    public char? CDel { get; set; }

    public string? STerm { get; set; }

    public DateTime? DStart { get; set; }

    public DateTime? DEnd { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
