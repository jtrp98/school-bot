using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotLevelProposal
{
    public int SchoolBotLevelProposalId { get; set; }

    public int CourseRunId { get; set; }

    public int TargetCurriculumId { get; set; }

    public string SnapshotHash { get; set; } = null!;

    public string Snapshot { get; set; } = null!;

    public string State { get; set; } = null!;

    public string? DecisionBy { get; set; }

    public DateTime? DecisionAt { get; set; }

    public string? Reason { get; set; }

    public int? SuccessorRunId { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
