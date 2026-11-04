using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class SchoolSetting
{
    public long Id { get; set; }

    public int SchoolId { get; set; }

    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }
}
