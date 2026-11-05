using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TfcmdeliveryAck
{
    public long Id { get; set; }

    public string RequestId { get; set; } = null!;

    public string? Token { get; set; }

    public int PushMid { get; set; }

    public string AckType { get; set; } = null!;

    public DateTime AckTime { get; set; }

    public string? DeviceInfo { get; set; }

    public DateTime CreatedDate { get; set; }
}
