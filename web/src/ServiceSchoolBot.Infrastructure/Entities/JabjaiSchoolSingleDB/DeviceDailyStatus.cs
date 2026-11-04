using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class DeviceDailyStatus
{
    public Guid DeviceStatusId { get; set; }

    public int SchoolId { get; set; }

    public string DeviceId { get; set; } = null!;

    public bool Online { get; set; }

    public DateTime? OnlineTime { get; set; }

    public bool Login { get; set; }

    public DateTime? LoginTime { get; set; }

    public bool LogOut { get; set; }

    public DateTime? LogoutTime { get; set; }

    public DateTime Tstamp { get; set; }

    public DateTime BusinessDate { get; set; }

    public string? AppVersion { get; set; }

    public string? AppName { get; set; }

    public string? Note { get; set; }

    public int? UserLoginId { get; set; }

    public int? DeviceTypeId { get; set; }

    public bool NotifyEnabled { get; set; }
}
