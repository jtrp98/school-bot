using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtassessmentType
{
    public int AssessmentTypeId { get; set; }

    public string? AssessmentTypeDesc { get; set; }

    public int? DisplayOrder { get; set; }

    public bool? IsActive { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
