using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class DeviceDetail
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public string? DeviceId { get; set; }

    public string? Status { get; set; }

    public DateTime? LastUpdated { get; set; }

    public string? AppVersion { get; set; }

    public string? Ipaddress { get; set; }

    public string? ServiceProvider { get; set; }
}
