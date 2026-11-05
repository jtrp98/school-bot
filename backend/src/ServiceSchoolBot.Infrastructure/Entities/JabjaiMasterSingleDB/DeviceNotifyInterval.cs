using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class DeviceNotifyInterval
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public int Round { get; set; }

    public string Label { get; set; } = null!;

    public int IntervalMinutes { get; set; }

    public bool IsActive { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
