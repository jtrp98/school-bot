using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TsubLevel
{
    public int NTsubLevel { get; set; }

    public string? SubLevel { get; set; }

    public int? NTlevel { get; set; }

    public int? NTimeType { get; set; }

    public int? NDeleted { get; set; }

    public string? FullName { get; set; }

    public string? SubLevelEn { get; set; }

    public string? FullNameEn { get; set; }

    public int? NWorkingStatus { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public bool? IsGraduate { get; set; }

    public string? MasterCode { get; set; }

    public string? SubLevelNameEn { get; set; }

    public string? SubLevelNameTh { get; set; }
}
