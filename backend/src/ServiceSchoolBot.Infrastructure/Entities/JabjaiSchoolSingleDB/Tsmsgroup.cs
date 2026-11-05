using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class Tsmsgroup
{
    public int SmsgroupId { get; set; }

    public string? SmsgroupName { get; set; }

    public int? Status { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? CDel { get; set; }

    public string? GroupDefault { get; set; }

    public string? SmsgroupNameEn { get; set; }
}
