using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcontrolJob
{
    public int ControlJobId { get; set; }

    public string JobName { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastRunDate { get; set; }
}
