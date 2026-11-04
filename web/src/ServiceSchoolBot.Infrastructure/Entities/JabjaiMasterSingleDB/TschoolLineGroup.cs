using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TschoolLineGroup
{
    public int SchoolLineGroupId { get; set; }

    public int? SchoolId { get; set; }

    public string? GroupId { get; set; }

    public string? GroupName { get; set; }

    public DateTime? InsertedDateTime { get; set; }
}
