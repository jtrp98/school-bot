using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tperiod
{
    public string SPeriodId { get; set; } = null!;

    public string? SScheduleId { get; set; }

    public int? SPlaneId { get; set; }

    public string? SClassId { get; set; }

    public int? NDay { get; set; }

    public string? SPeriodName { get; set; }

    public DateTime? DStart { get; set; }

    public DateTime? DEnd { get; set; }

    public DateTime? DTimeStartIn { get; set; }

    public DateTime? DTimeStartOut { get; set; }

    public DateTime? DTimeEndIn { get; set; }

    public DateTime? DTimeEndOut { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
