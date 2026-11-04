using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TgradeMigration
{
    public int GradeMigrationId { get; set; }

    public int NGradeId { get; set; }

    public int SId { get; set; }

    public int AssessmentId { get; set; }

    public int ExamId { get; set; }

    public int NTermSubLevel2 { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public string? ApplicationName { get; set; }
}
