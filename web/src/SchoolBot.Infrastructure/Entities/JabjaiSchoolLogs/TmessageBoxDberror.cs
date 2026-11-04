using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TmessageBoxDberror
{
    public Guid Id { get; set; }

    public string? MessageBoxData { get; set; }

    public DateTime Tstamp { get; set; }

    public int? SellId { get; set; }

    public int SchoolId { get; set; }

    public string? ErrorDescription { get; set; }

    public bool Processed { get; set; }

    public string? MessageBoxUserData { get; set; }
}
