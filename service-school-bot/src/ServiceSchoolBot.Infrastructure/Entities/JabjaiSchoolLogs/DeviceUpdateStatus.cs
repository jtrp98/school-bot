using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class DeviceUpdateStatus
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public string DeviceId { get; set; } = null!;

    public int UserId { get; set; }

    public bool NeedToUpdate { get; set; }

    public DateTime Tstamp { get; set; }

    public bool ResponseStatus { get; set; }
}
