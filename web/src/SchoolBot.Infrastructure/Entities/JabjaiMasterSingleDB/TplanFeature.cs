using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TplanFeature
{
    public int PlanId { get; set; }

    public int FeatureId { get; set; }

    public string? DisplayIcon { get; set; }

    public string? DisplayTitle { get; set; }

    public string? DisplayDescription { get; set; }

    public int SortOrder { get; set; }

    public string? SchoolId { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
