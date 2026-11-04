using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TgroupPermissionLog
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public DateTime? Date { get; set; }

    public string? Text { get; set; }

    public string? ByUser { get; set; }
}
