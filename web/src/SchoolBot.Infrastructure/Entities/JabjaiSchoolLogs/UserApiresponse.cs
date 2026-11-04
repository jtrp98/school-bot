using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class UserApiresponse
{
    public int Id { get; set; }

    public int? SchoolId { get; set; }

    public string? MethodName { get; set; }

    public DateTime? DeviceTime { get; set; }

    public double? ResponseTime { get; set; }

    public DateTime? ApiprocessedTime { get; set; }

    public DateTime? Tstamp { get; set; }

    public int? EmployeeId { get; set; }

    public int? ShopId { get; set; }

    public int? SId { get; set; }

    public int? SEmp { get; set; }

    public int? UserId { get; set; }

    public string? Status { get; set; }

    public string? ApplicationName { get; set; }
}
