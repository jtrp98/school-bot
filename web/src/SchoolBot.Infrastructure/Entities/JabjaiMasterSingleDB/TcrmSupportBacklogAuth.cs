using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcrmSupportBacklogAuth
{
    public int CrmSupportBacklogAuthId { get; set; }

    public int? StaffId { get; set; }

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? ExpireDate { get; set; }
}
