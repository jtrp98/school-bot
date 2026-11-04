using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TblMedicalProfileVaccination
{
    public long Id { get; set; }

    public int SchoolId { get; set; }

    public int UserId { get; set; }

    public long VaccineId { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TblMasterVaccine Vaccine { get; set; } = null!;
}
