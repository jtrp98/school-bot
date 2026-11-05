using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class DeviceType
{
    public int DeviceTypeId { get; set; }

    public string? TypeNameEn { get; set; }

    public string? TypeNameTh { get; set; }
}
