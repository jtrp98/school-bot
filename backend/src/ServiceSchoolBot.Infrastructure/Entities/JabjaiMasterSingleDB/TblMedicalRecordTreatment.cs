using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TblMedicalRecordTreatment
{
    public long Id { get; set; }

    public int SchoolId { get; set; }

    public long TblMedicalRecordId { get; set; }

    public long TreatmentId { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TblMedicalRecord TblMedicalRecord { get; set; } = null!;

    public virtual TblMasterTreatment Treatment { get; set; } = null!;
}
