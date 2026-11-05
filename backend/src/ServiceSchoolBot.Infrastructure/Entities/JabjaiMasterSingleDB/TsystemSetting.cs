using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TsystemSetting
{
    public int NSystemId { get; set; }

    public int? SchoolId { get; set; }

    public bool? BScanOut { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? MenuId { get; set; }

    public string? DocumentId { get; set; }

    public string? Config { get; set; }

    public bool? IsOpenApproveUserProfile { get; set; }

    public int? ApproveOption { get; set; }

    public DateTime? ApproveStartDate { get; set; }

    public DateTime? ApproveEndDate { get; set; }

    public double? MaxTopup { get; set; }
}
