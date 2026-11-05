using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbAppVersion
{
    public Guid FdId { get; set; }

    public string FdName { get; set; } = null!;

    public Guid FdAppId { get; set; }

    public string? FdUrl { get; set; }

    public bool? FdProd { get; set; }

    public bool? FdDev { get; set; }

    public string? FdEnvironment { get; set; }

    public bool? FdIsLastestVersion { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
