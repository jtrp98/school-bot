using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class LessonPlanFile
{
    public int LessonPlanFileId { get; set; }

    public int SchoolId { get; set; }

    public int? LessonPlanSubmissionId { get; set; }

    public string FileName { get; set; } = null!;

    public string ObjectKey { get; set; } = null!;

    public string MimeType { get; set; } = null!;

    public int SizeBytes { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool Cdel { get; set; }

    public virtual LessonPlanSubmission? LessonPlanSubmission { get; set; }
}
