using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TgroupPermissionMenu
{
    public int MenuId { get; set; }

    public int GroupId { get; set; }

    public char Type { get; set; }

    public short? Role { get; set; }

    public int? SchoolId { get; set; }

    public bool? IsDel { get; set; }

    public bool? IsActive { get; set; }
}
