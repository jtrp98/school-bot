using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TpromoteBatchDetail
{
    public long Id { get; set; }

    public Guid BatchId { get; set; }

    public int SchoolId { get; set; }

    public int CurrentYearId { get; set; }

    public string CurrentTermId { get; set; } = null!;

    public int TargetYearId { get; set; }

    public string TargetTermId { get; set; } = null!;

    public int CurrentLevelId { get; set; }

    public int CurrentRoomId { get; set; }

    public int TargetLevelId { get; set; }

    public int TargetRoomId { get; set; }

    public int TotalStudents { get; set; }

    public string SIds { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public string CreatedByName { get; set; } = null!;

    public string? RolledBackStatus { get; set; }

    public DateTime? RolledBackDate { get; set; }

    public int? RolledBackBy { get; set; }
}
