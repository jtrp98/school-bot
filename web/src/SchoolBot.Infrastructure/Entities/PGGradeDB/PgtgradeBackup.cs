using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtgradeBackup
{
    public Guid BackupId { get; set; }

    public int SchoolId { get; set; }

    public string NTerm { get; set; } = null!;

    public int SourceNtermSubLevel2 { get; set; }

    public int? DestinationNtermSubLevel2 { get; set; }

    public int? SId { get; set; }

    public string BackupType { get; set; } = null!;

    public int GradeCount { get; set; }

    public int DetailCount { get; set; }

    public int AttendanceCount { get; set; }

    public string SnapshotData { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? Note { get; set; }
}
