using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbChangePasswordHistory
{
    public Guid FdChangePasswordId { get; set; }

    public DateTime? FdCreateDate { get; set; }

    public int? FdUserId { get; set; }

    public DateTime? FdResetDate { get; set; }

    public string? FdOldPasswrod { get; set; }

    public string? FdNewPassword { get; set; }

    public int? FdSchoolId { get; set; }
}
