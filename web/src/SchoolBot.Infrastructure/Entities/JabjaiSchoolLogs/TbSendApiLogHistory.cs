using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TbSendApiLogHistory
{
    public int Id { get; set; }

    public string? ApiName { get; set; }

    public string? Info { get; set; }

    public string? Result { get; set; }

    public int? SchoolId { get; set; }

    public int? ResponseTime { get; set; }

    public DateTime? Tstamp { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid Guid { get; set; }
}
