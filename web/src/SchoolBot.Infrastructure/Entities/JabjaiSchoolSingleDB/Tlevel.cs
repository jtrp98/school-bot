using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class Tlevel
{
    public int LevelId { get; set; }

    public string? LevelName { get; set; }

    public int? NTimeType { get; set; }

    public int? SortValue { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public string? MasterCode { get; set; }

    public string? LevelNameEng { get; set; }
}
