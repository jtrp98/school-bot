using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TstudentType
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public string? StudentTypeTh { get; set; }

    public string? StudentTypeEn { get; set; }

    public bool? Status { get; set; }

    public bool? IsDel { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }
}
