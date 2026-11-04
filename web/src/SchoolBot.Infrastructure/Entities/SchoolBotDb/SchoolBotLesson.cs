using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotLesson
{
    public int SchoolBotLessonId { get; set; }

    public int CurriculumId { get; set; }

    public Guid LessonKey { get; set; }

    public int Ordinal { get; set; }

    public string Title { get; set; } = null!;

    public string Objective { get; set; } = null!;

    public string Instructions { get; set; } = null!;

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? SchoolId { get; set; }
}
