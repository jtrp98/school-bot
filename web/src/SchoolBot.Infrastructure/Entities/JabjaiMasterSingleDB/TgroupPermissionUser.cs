using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TgroupPermissionUser
{
    public int GroupId { get; set; }

    public int UserId { get; set; }

    public bool? IsDel { get; set; }

    public bool? IsActive { get; set; }

    public int? SchoolId { get; set; }
}
