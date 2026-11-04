using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class District
{
    public int DistrictId { get; set; }

    public string DistrictCode { get; set; } = null!;

    public string? DistrictNameEn { get; set; }

    public string DistrictName { get; set; } = null!;

    public int AmphurId { get; set; }

    public int ProvinceId { get; set; }

    public int GeoId { get; set; }
}
