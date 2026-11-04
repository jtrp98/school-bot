using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TlogDebug
{
    public int Id { get; set; }

    public int? SchoolId { get; set; }

    public int? StudentId { get; set; }

    public int? EmployeeId { get; set; }

    public string? LogMessage { get; set; }

    public DateTime? LogDate { get; set; }

    public string? Ip { get; set; }
}
