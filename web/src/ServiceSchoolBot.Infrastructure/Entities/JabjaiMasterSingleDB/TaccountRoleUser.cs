using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TaccountRoleUser
{
    public int AccountRoleUserId { get; set; }

    public int? AccountRoleGroupId { get; set; }

    public int? UserId { get; set; }
}
