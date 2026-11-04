using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TparentStudent
{
    public int ParentStudentId { get; set; }

    public int ParentId { get; set; }

    public int SchoolId { get; set; }

    public int SId { get; set; }

    public string? ParentCode { get; set; }

    public string? Relation { get; set; }

    public string? RelationOtherText { get; set; }

    public DateTime? LinkedAt { get; set; }

    public string? LinkedBy { get; set; }

    public DateTime? UnlinkedAt { get; set; }

    public string? UnlinkedBy { get; set; }

    public virtual Tparent Parent { get; set; } = null!;
}
