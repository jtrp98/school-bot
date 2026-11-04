using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TtempScanCameraIpstatus
{
    public int SchoolId { get; set; }

    public string? Ip { get; set; }

    public string? Mac { get; set; }

    public string? MachineName { get; set; }

    public int? CameraNo { get; set; }

    public string CameraIp { get; set; } = null!;

    public string? CameraStatus { get; set; }

    public DateTime? LastUpdate { get; set; }
}
