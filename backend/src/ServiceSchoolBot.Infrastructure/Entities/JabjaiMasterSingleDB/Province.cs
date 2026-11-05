using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Province
{
    public int ProvinceId { get; set; }

    public string ProvinceCode { get; set; } = null!;

    public string ProvinceName { get; set; } = null!;

    public int GeoId { get; set; }

    public string? ProvinceNameEn { get; set; }
}
