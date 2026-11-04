using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtacademicSetting
{
    public int SettingId { get; set; }

    public int SchoolId { get; set; }

    public string SettingKey { get; set; } = null!;

    public string SettingValue { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool? IsDeleted { get; set; }
}
