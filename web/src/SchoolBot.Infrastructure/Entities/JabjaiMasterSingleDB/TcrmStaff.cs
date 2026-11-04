using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcrmStaff
{
    public int CrmStaffId { get; set; }

    public int? StaffId { get; set; }

    public string? Status { get; set; }

    public int? MaxConcurrent { get; set; }

    public DateTime? UpdateDate { get; set; }
}
