using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tfeature
{
    public int FeatureId { get; set; }

    public string? GroupName { get; set; }

    public string? FeatureName { get; set; }

    public string? Type { get; set; }

    public string? QueryParams { get; set; }

    public string? Keyword { get; set; }

    public bool? Active { get; set; }
}
