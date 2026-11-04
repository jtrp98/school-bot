using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbPaymentSetting
{
    public Guid SystemId { get; set; }

    public string? Bank { get; set; }

    public string? SystemName { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }
}
