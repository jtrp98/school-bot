using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class DeviceAnydesk
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public string DeviceId { get; set; } = null!;

    public string AnydeskId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
