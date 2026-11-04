using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class DeviceNotifyTimeWindow
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public int Round { get; set; }

    public string Label { get; set; } = null!;

    public int StartHour { get; set; }

    public int StartMin { get; set; }

    public int EndHour { get; set; }

    public int EndMin { get; set; }

    public bool IsActive { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
