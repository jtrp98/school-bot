using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcontrolJobLog
{
    public int ControlJobLogId { get; set; }

    public string? ControlJob { get; set; }

    public string? Parameter { get; set; }

    public string? Error { get; set; }

    public DateTime? CreateDate { get; set; }
}
