using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtgradeSetting
{
    public int GradeSettingId { get; set; }

    public int GradeTemplateId { get; set; }

    public string TemplateGrades { get; set; } = null!;

    public int Nyear { get; set; }

    public string NumberYear { get; set; } = null!;

    public int ClassLevelId { get; set; }

    public string ClassLevelName { get; set; } = null!;

    public int SchoolId { get; set; }

    public bool IsLocked { get; set; }

    public bool CDel { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string SettingMode { get; set; } = null!;
}
