using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbLoginHistory
{
    public Guid FdLoginHistoryId { get; set; }

    public DateTime? FdLoginDate { get; set; }

    public string? FdToken { get; set; }

    public int? FdUserId { get; set; }

    public int? FdSchoolId { get; set; }

    public string? Imei { get; set; }

    public string? System { get; set; }

    public string? FdVersion { get; set; }

    public string? FdMbbrand { get; set; }
}
