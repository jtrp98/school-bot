using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TsendSubjectPsi
{
    public int SchoolId { get; set; }

    public int SPlaneId { get; set; }

    public DateTime? SendDate { get; set; }

    public int? StatusCode { get; set; }

    public string? ResponseContent { get; set; }
}
