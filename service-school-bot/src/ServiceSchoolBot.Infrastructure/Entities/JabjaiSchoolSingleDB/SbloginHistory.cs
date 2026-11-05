using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class SbloginHistory
{
    public int Id { get; set; }

    public int? NStudentId { get; set; }

    public string? SFirstName { get; set; }

    public string? SLastName { get; set; }

    public int? SchoolId { get; set; }

    public string? SImei { get; set; }

    public string? SDeviceOs { get; set; }

    public string? SAppName { get; set; }

    public string? SAppVersion { get; set; }

    public string? SIpaddress { get; set; }

    public DateTime? LoginAt { get; set; }
}
