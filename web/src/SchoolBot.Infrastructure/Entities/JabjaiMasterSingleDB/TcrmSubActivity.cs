using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcrmSubActivity
{
    public int CrmSubActivityId { get; set; }

    public int? CrmActivityId { get; set; }

    public int? SchoolId { get; set; }

    public int? ContactId { get; set; }

    public int? CrmContactId { get; set; }

    public int? CrmSubContactId { get; set; }

    public DateTime? ImportDate { get; set; }

    public int? ImportBy { get; set; }

    public string? SchoolName { get; set; }

    public string? Position { get; set; }

    public string? Name { get; set; }

    public string? Tel { get; set; }
}
