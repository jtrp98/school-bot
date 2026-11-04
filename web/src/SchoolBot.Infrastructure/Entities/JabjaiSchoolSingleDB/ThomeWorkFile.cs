using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class ThomeWorkFile
{
    public int? NHomeWorkId { get; set; }

    public string? SFileName { get; set; }

    public string? ContentType { get; set; }

    public int NFileId { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public string? Title { get; set; }
}
