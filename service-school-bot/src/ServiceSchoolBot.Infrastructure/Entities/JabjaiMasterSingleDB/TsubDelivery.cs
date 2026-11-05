using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TsubDelivery
{
    public int SubDeliveryId { get; set; }

    public int? DeliveryId { get; set; }

    public string? ProductName { get; set; }

    public int? Qty { get; set; }

    public string? SerialNo { get; set; }

    public DateTime? StartDate { get; set; }

    public string? InsuranceNo { get; set; }

    public DateTime? EndDate { get; set; }

    public string? DeliveryNo { get; set; }

    public string? ClaimNo { get; set; }

    public string? Note { get; set; }
}
