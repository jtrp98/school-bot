using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotActivity
{
    public int SchoolBotActivityId { get; set; }

    public int LessonId { get; set; }

    public Guid ActivityKey { get; set; }

    public int Ordinal { get; set; }

    public string Title { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string? Mode { get; set; }

    public string Indicators { get; set; } = null!;

    public string Config { get; set; } = null!;

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? SchoolId { get; set; }
}
