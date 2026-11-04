using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcrmAssignSupport
{
    public int CrmAssignSupportId { get; set; }

    public int? CrmSupportId { get; set; }

    public int? StaffId { get; set; }

    public DateTime? CreateDate { get; set; }
}
