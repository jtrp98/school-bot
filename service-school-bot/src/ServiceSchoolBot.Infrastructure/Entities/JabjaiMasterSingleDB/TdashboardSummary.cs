using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TdashboardSummary
{
    public int DashboardSummaryId { get; set; }

    public string? SchoolSummary { get; set; }

    public string? StudentSummary { get; set; }

    public string? TeacherSummary { get; set; }

    public DateTime? UpdateDate { get; set; }
}
