using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TlineGroup
{
    public int LineGroupId { get; set; }

    public int? SchoolId { get; set; }

    public string? GroupId { get; set; }

    public string? LineNotificationAccessToken { get; set; }

    public string? GroupType { get; set; }

    public DateTime? CreateDate { get; set; }
}
