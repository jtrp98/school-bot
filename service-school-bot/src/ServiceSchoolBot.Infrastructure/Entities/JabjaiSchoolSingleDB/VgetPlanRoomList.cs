using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class VgetPlanRoomList
{
    public int? PlanId { get; set; }

    public string? PlanName { get; set; }

    public string? SubLevel { get; set; }

    public string? NTsubLevel2 { get; set; }

    public int? SortBy { get; set; }

    public int? NTermSubLevel2 { get; set; }

    public int? SchoolId { get; set; }
}
