using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class ViewActiveSchoolList
{
    public int? NCompany { get; set; }

    public string? SCompany { get; set; }

    public bool? CDel { get; set; }

    public bool? Active { get; set; }

    public bool? IsActive { get; set; }
}
