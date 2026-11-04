using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotMission
{
    public int SchoolBotMissionId { get; set; }

    public int LessonId { get; set; }

    public Guid MissionKey { get; set; }

    public int Ordinal { get; set; }

    public string Title { get; set; } = null!;

    public string Instructions { get; set; } = null!;

    public string Method { get; set; } = null!;

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? SchoolId { get; set; }
}
