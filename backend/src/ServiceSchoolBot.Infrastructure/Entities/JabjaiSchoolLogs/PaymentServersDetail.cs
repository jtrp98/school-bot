using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class PaymentServersDetail
{
    public int Id { get; set; }

    public string Ipaddress { get; set; } = null!;

    public bool Updated { get; set; }

    public DateTime Tstamp { get; set; }

    public bool IsDevMode { get; set; }
}
