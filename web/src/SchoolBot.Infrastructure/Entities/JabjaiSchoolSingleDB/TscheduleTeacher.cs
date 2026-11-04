using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TscheduleTeacher
{
    public int ScheduleTeacherId { get; set; }

    public int? SScheduleId { get; set; }

    public bool? CDel { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? SEmp { get; set; }
}
