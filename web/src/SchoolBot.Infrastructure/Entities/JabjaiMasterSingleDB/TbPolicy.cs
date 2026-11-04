using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbPolicy
{
    public Guid FdPolicyId { get; set; }

    public string? FdMessage { get; set; }

    public string? FdVersion { get; set; }

    public bool? FdIsActive { get; set; }

    public bool? FdIsDel { get; set; }

    public int? FdIsStatus { get; set; }

    public DateTime? FdCreated { get; set; }

    public DateTime? FdModified { get; set; }

    public Guid? FdCreateBy { get; set; }

    public Guid? FdModifyBy { get; set; }
}
