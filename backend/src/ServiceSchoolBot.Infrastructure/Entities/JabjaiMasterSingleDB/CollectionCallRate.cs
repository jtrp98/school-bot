using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class CollectionCallRate
{
    public Guid RateId { get; set; }

    public int? SchoolId { get; set; }

    public decimal RatePerCall { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public string? Note { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreateDate { get; set; }
}
