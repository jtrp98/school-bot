using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TaccountRoleGroup
{
    public int AccountRoleGroupId { get; set; }

    public string? RoleGroupName { get; set; }

    public string? Note { get; set; }
}
