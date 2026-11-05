using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TmobileMenu
{
    public int MenuId { get; set; }

    public string? MenuName { get; set; }

    public string? MenuNameEn { get; set; }

    public bool? Actvie { get; set; }

    public bool? Demo { get; set; }

    public int? SubMenuId { get; set; }

    public bool? IsExceptAuth { get; set; }

    public short? OrderNo { get; set; }

    public string? MenuUrl { get; set; }
}
