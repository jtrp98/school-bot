using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TstudentDisability
{
    public int SId { get; set; }

    public int SchoolId { get; set; }

    public bool? HasDisabilityCard { get; set; }

    public string? DisabilityCardNo { get; set; }

    public DateTime? DisabilityCardExpireDate { get; set; }

    public bool? HasDisabilityCert { get; set; }

    public DateTime? DisabilityCertIssueDate { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
