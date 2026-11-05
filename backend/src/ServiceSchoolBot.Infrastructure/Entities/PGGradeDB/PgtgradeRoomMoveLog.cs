using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtgradeRoomMoveLog
{
    public int LogId { get; set; }

    public Guid RequestId { get; set; }

    public int SchoolId { get; set; }

    public string NTerm { get; set; } = null!;

    public int SourceNtermSubLevel2 { get; set; }

    public int DestinationNtermSubLevel2 { get; set; }

    public int SId { get; set; }

    public int SPlaneId { get; set; }

    public int SourceNgradeId { get; set; }

    public int DestinationNgradeId { get; set; }

    public int NgradeDetailId { get; set; }

    public string? OldScoreData { get; set; }

    public string? NewScoreData { get; set; }

    public int? DestOldGradeDetailId { get; set; }

    public string? DestOldScoreData { get; set; }

    public int? SourceOldPlanId { get; set; }

    public string Status { get; set; } = null!;

    public string? Reason { get; set; }

    public string? ReasonDetail { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }
}
