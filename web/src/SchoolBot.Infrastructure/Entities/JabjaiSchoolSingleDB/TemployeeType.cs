using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TemployeeType
{
    public int NTypeId { get; set; }

    public int? NTypeId2 { get; set; }

    public string? Title { get; set; }

    public int SchoolId { get; set; }

    public bool IsDel { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? MasterCode { get; set; }
}
