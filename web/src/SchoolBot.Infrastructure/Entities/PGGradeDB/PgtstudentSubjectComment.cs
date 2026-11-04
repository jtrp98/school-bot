using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtstudentSubjectComment
{
    public int CommentId { get; set; }

    public int? SId { get; set; }

    public string? Nterm { get; set; }

    public int SPlaneId { get; set; }

    public string? Comment { get; set; }

    public bool? CDel { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? SchoolId { get; set; }
}
