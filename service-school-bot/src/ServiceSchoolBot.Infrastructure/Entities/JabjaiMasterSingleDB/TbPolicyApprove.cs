using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbPolicyApprove
{
    public Guid FdApproveId { get; set; }

    public Guid FdPolicyId { get; set; }

    public int FdUserId { get; set; }

    public DateTime? FdApproveDate { get; set; }

    public short? FdUserType { get; set; }
}
