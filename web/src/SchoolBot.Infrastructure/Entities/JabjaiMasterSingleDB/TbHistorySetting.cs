using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbHistorySetting
{
    public Guid FdHistoryId { get; set; }

    public string? FdFunctionName { get; set; }

    public string? FdSettingData { get; set; }

    public int? FdSchoolId { get; set; }

    public DateTime? FdUpdatedDate { get; set; }

    public int? FdUpdatedBy { get; set; }
}
