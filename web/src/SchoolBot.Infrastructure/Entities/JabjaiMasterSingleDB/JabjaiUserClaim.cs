using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class JabjaiUserClaim
{
    public int UserId { get; set; }

    public int SchoolId { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? AuthorizationKey { get; set; }

    public string? OldAuthorizationKey { get; set; }

    public DateTime ExpiredDate { get; set; }

    public DateTime Tstamp { get; set; }

    public DateTime GeneratedDate { get; set; }

    public short? UserType { get; set; }
}
