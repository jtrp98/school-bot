using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class DeviceType
{
    public int DeviceTypeId { get; set; }

    public string? TypeNameEn { get; set; }

    public string? TypeNameTh { get; set; }
}
