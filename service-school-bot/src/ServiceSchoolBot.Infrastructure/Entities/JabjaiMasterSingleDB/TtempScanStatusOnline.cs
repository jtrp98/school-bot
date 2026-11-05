using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TtempScanStatusOnline
{
    public int SchoolId { get; set; }

    public string Ip { get; set; } = null!;

    public string? Mac { get; set; }

    public string? MachineName { get; set; }

    public DateTime? LastOnline { get; set; }

    public int CameraNo { get; set; }

    public string CameraIp { get; set; } = null!;

    public string? CameraStatus { get; set; }
}
