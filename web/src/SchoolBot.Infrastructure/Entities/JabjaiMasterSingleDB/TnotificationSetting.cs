using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TnotificationSetting
{
    public int NotificationSettingId { get; set; }

    public int? SchoolId { get; set; }

    public long? StaffId { get; set; }

    public string? LineNotificationAccessToken { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }
}
