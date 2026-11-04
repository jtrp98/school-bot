using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TgroupPermission
{
    public int GroupId { get; set; }

    public string? GroupName { get; set; }

    public bool? IsDel { get; set; }

    public bool? IsActive { get; set; }

    public int? SchoolId { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }

    public bool? IsEditable { get; set; }
}
