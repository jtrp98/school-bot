using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TblMedicalRecordMedicine
{
    public long Id { get; set; }

    public int SchoolId { get; set; }

    public long TblMedicalRecordId { get; set; }

    public long MedicineId { get; set; }

    public decimal Qty { get; set; }

    public long? UnitId { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TblMasterMedicine Medicine { get; set; } = null!;

    public virtual TblMedicalRecord TblMedicalRecord { get; set; } = null!;

    public virtual TblMasterUnit? Unit { get; set; }
}
