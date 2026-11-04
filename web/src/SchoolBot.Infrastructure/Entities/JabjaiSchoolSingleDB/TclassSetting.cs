using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TclassSetting
{
    public int ClassSettingId { get; set; }

    public int SchoolId { get; set; }

    public int NYear { get; set; }

    public string NTerm { get; set; } = null!;

    public string SClassId { get; set; } = null!;

    public int? NCapacity { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
