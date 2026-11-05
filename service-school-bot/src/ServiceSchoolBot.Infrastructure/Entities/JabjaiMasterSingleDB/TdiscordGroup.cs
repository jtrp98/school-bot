using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TdiscordGroup
{
    public int DiscordGroupId { get; set; }

    public int? SchoolId { get; set; }

    public string? GroupName { get; set; }

    public string? WebhookUrl { get; set; }

    public DateTime? CreateDate { get; set; }
}
