using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbBehaviorJob
{
    public Guid Id { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? JobName { get; set; }

    public string? JobValues { get; set; }

    public string? JobStatus { get; set; }

    public int? SchoolId { get; set; }

    public DateTime? LogDate { get; set; }
}
