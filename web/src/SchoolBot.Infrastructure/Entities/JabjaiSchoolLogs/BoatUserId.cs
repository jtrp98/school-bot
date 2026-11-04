using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class BoatUserId
{
    public int Id { get; set; }

    public int? SId { get; set; }

    public string? Type { get; set; }

    public int SSellId { get; set; }

    public int MoneyId { get; set; }

    public DateTime Tstamp { get; set; }

    public Guid? CardId { get; set; }
}
