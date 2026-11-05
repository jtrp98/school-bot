using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class DeviceNotifyState
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public string DeviceId { get; set; } = null!;

    public DateTime? OfflineSince { get; set; }

    public DateTime? R1SentAt { get; set; }

    public DateTime? LastNotifiedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
