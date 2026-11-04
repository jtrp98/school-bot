using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TfeatureSbplu
{
    public int FeatureId { get; set; }

    public string FeatureDescription { get; set; } = null!;

    public string? DefaultIcon { get; set; }

    public string? DefaultTitle { get; set; }

    public string? SchoolId { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? FeatureCode { get; set; }
}
