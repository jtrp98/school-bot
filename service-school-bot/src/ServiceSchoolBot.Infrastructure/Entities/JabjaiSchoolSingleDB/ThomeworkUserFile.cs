using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class ThomeworkUserFile
{
    public int FileId { get; set; }

    public int NHomeWorkId { get; set; }

    public int SId { get; set; }

    public int SchoolId { get; set; }

    public string? FileUrl { get; set; }

    public string? FileTitle { get; set; }

    public string? FileContentType { get; set; }

    public bool? CDel { get; set; }
}
