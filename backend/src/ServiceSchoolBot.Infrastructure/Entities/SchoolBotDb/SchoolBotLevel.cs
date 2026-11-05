using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotLevel
{
    public int SchoolBotLevelId { get; set; }

    public Guid LevelKey { get; set; }

    public string Code { get; set; } = null!;

    public int Ordinal { get; set; }

    public string Title { get; set; } = null!;

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? SchoolId { get; set; }
}
