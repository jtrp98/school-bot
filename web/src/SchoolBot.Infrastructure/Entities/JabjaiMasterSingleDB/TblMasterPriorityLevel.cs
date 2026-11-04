using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TblMasterPriorityLevel
{
    public long Id { get; set; }

    public int? SchoolId { get; set; }

    public string LevelGroup { get; set; } = null!;

    public string Code { get; set; } = null!;

    public short Level { get; set; }

    public string? ColorHex { get; set; }

    public string NameI18n { get; set; } = null!;

    public bool IsActive { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public short? SeqNo { get; set; }

    public virtual ICollection<TblMasterSymptom> TblMasterSymptoms { get; set; } = new List<TblMasterSymptom>();

    public virtual ICollection<TblMedicalProfileAccident> TblMedicalProfileAccidents { get; set; } = new List<TblMedicalProfileAccident>();

    public virtual ICollection<TblMedicalProfileAllergy> TblMedicalProfileAllergies { get; set; } = new List<TblMedicalProfileAllergy>();

    public virtual ICollection<TblMedicalProfileChronicDisease> TblMedicalProfileChronicDiseases { get; set; } = new List<TblMedicalProfileChronicDisease>();

    public virtual ICollection<TblMedicalRecord> TblMedicalRecords { get; set; } = new List<TblMedicalRecord>();
}
