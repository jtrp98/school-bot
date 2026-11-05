using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TdbjobLog
{
    public int Id { get; set; }

    public string JobName { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? JsonData { get; set; }

    public string? ErrorDescription { get; set; }

    public DateTime? Tstamp { get; set; }

    public DateOnly BusinessDate { get; set; }

    public string? Type { get; set; }

    public int? AvgWorkingTime { get; set; }

    public int? TodayWorkingTime { get; set; }

    public int? YesterdayWorkingTime { get; set; }

    public string? WorkingStatus { get; set; }

    public string? BoatType { get; set; }

    public int? SchoolId { get; set; }

    public string? JobDescription { get; set; }
}
