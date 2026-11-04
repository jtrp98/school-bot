using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class UserUsedLimit
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string UserType { get; set; } = null!;

    public decimal UsedAmount { get; set; }

    public DateTime Tstamp { get; set; }
}
