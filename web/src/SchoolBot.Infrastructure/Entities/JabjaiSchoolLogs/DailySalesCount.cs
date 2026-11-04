using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class DailySalesCount
{
    public int Id { get; set; }

    public int? ShopId { get; set; }

    public string? DeviceId { get; set; }

    public string? SalesData { get; set; }

    public DateTime? Tstamp { get; set; }

    public int RecordCount { get; set; }
}
