using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TblUserBypass
{
    public string SchoolId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public bool BypassPassword { get; set; }

    public bool Bypass2fa { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
