using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtgradeTemplate
{
    public int GradeTemplateId { get; set; }

    public string? TemplateName { get; set; }

    public string TemplateGrades { get; set; } = null!;

    public int? SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
