using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TschoolRecord
{
    public int NSchoolRecordId { get; set; }

    public string? NTerm { get; set; }

    public int? NTsudentId { get; set; }

    public DateTime? DAdd { get; set; }

    public DateTime? DUpdate { get; set; }

    public int? UserAdd { get; set; }

    public int? UserUpdate { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
