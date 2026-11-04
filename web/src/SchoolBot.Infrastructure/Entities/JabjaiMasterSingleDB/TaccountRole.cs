using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TaccountRole
{
    public int AccountRoleId { get; set; }

    public int? PageGroupId { get; set; }

    public int? PageId { get; set; }

    public int? AccountRoleGroupId { get; set; }

    public bool? CanModify { get; set; }

    public bool? CanView { get; set; }

    public bool? NoPermission { get; set; }

    public bool? IsDefault { get; set; }

    public int? RoleValue { get; set; }
}
