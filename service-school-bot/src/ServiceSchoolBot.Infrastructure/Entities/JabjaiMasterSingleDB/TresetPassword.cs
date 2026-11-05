using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TresetPassword
{
    public int Id { get; set; }

    public int? SUerId { get; set; }

    public DateTime? DExpUpdate { get; set; }

    public DateTime? DExpActive { get; set; }

    public string? Otp { get; set; }

    public string? CStatus { get; set; }
}
