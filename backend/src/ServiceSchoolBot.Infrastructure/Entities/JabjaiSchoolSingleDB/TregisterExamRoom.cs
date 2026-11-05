using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TregisterExamRoom
{
    public int RegisterExamRoomId { get; set; }

    public int? NTsubLevel { get; set; }

    public int? RegisterPlanSetupId { get; set; }

    public string? ExamRoomName { get; set; }

    public int? Seats { get; set; }

    public int SchoolId { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public bool? IsDel { get; set; }
}
