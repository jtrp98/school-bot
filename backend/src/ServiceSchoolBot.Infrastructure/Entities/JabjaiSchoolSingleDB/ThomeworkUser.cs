using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class ThomeworkUser
{
    public int NHomeWork { get; set; }

    public int SId { get; set; }

    public char? CStatusHomeWork { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public bool? IsRead { get; set; }

    public bool? IsSend { get; set; }

    public bool? IsLate { get; set; }

    public bool? IsManual { get; set; }

    public double? Score { get; set; }

    public string? AttachFile { get; set; }

    public string? AttachTitle { get; set; }

    public string? AttachFileContentType { get; set; }

    public string? LinkUrl { get; set; }

    public string? Remark { get; set; }
}
