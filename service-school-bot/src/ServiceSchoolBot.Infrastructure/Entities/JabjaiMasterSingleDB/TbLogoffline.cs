using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbLogoffline
{
    public Guid LogOfflineId { get; set; }

    public int SchoolId { get; set; }

    public string? LogText { get; set; }

    public DateTime? Created { get; set; }
}
