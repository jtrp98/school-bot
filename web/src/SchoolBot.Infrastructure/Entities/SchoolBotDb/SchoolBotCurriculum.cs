using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotCurriculum
{
    public int SchoolBotCurriculumId { get; set; }

    public Guid CurriculumKey { get; set; }

    public int Revision { get; set; }

    public int LevelId { get; set; }

    public string Title { get; set; } = null!;

    public string RecommendedGrades { get; set; } = null!;

    public string Subjects { get; set; } = null!;

    public string SourceRef { get; set; } = null!;

    public string Lifecycle { get; set; } = null!;

    public DateTime? PublishedAt { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? SchoolId { get; set; }
}
