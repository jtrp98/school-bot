using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TuserVerifyLineconnect
{
    public int ConnectId { get; set; }

    public int SchoolId { get; set; }

    public int UserId { get; set; }

    public string? LineauthorizeCode { get; set; }

    public string? Linetoken { get; set; }

    public DateTime? ConnectDate { get; set; }

    public int Status { get; set; }
}
