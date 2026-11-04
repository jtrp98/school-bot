using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TblMasterAllergyType
{
    public long Id { get; set; }

    public int? SchoolId { get; set; }

    public string Code { get; set; } = null!;

    public string NameI18n { get; set; } = null!;

    public bool IsActive { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public short? SeqNo { get; set; }

    public virtual ICollection<TblMasterAllergy> TblMasterAllergies { get; set; } = new List<TblMasterAllergy>();
}
