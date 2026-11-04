using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TblMedicineUnitMapping
{
    public long Id { get; set; }

    public int? SchoolId { get; set; }

    public long MedicineId { get; set; }

    public long UnitId { get; set; }

    public bool IsDefaultUnit { get; set; }

    public bool IsActive { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public short? SeqNo { get; set; }

    public virtual TblMasterMedicine Medicine { get; set; } = null!;

    public virtual TblMasterUnit Unit { get; set; } = null!;
}
