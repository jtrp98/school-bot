using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TblMedicalProfileChronicDisease
{
    public long Id { get; set; }

    public int SchoolId { get; set; }

    public int UserId { get; set; }

    public long DiseaseId { get; set; }

    public long? SeverityLevelId { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TblMasterDisease Disease { get; set; } = null!;

    public virtual TblMasterPriorityLevel? SeverityLevel { get; set; }
}
