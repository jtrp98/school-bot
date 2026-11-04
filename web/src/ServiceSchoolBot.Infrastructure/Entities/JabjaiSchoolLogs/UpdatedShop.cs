using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class UpdatedShop
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public string DeviceId { get; set; } = null!;

    public int UserId { get; set; }

    public int ShopId { get; set; }
}
