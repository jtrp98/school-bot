using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TblMedicalRecord
{
    public long Id { get; set; }

    public int SchoolId { get; set; }

    public int UserId { get; set; }

    public string TermId { get; set; } = null!;

    public long VisitTypeId { get; set; }

    public DateOnly VisitDate { get; set; }

    public TimeOnly VisitTime { get; set; }

    public TimeOnly? LeaveTime { get; set; }

    public string? SymOther { get; set; }

    public string? Note { get; set; }

    public long? DispenserId { get; set; }

    public long? EmergencyLevelId { get; set; }

    public long? StatusId { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TblMasterPriorityLevel? EmergencyLevel { get; set; }

    public virtual ICollection<TblMedicalRecordAttachment> TblMedicalRecordAttachments { get; set; } = new List<TblMedicalRecordAttachment>();

    public virtual ICollection<TblMedicalRecordMedicine> TblMedicalRecordMedicines { get; set; } = new List<TblMedicalRecordMedicine>();

    public virtual ICollection<TblMedicalRecordSymptom> TblMedicalRecordSymptoms { get; set; } = new List<TblMedicalRecordSymptom>();

    public virtual ICollection<TblMedicalRecordTreatment> TblMedicalRecordTreatments { get; set; } = new List<TblMedicalRecordTreatment>();

    public virtual TblMasterVisitType VisitType { get; set; } = null!;
}
