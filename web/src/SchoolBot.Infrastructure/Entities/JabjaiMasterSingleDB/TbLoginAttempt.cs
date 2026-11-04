using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbLoginAttempt
{
    public long Id { get; set; }

    public int UserId { get; set; }

    public int SchoolId { get; set; }

    public string Username { get; set; } = null!;

    public DateTime AttemptDate { get; set; }

    public bool IsSuccess { get; set; }

    public string? FailureReason { get; set; }

    public bool IsAccountLocked { get; set; }

    public bool RequirePasswordChange { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? Ipaddress { get; set; }
}
