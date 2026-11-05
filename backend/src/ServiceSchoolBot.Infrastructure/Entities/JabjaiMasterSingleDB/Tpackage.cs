using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tpackage
{
    public int PackageId { get; set; }

    public string? PackageName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }
}
