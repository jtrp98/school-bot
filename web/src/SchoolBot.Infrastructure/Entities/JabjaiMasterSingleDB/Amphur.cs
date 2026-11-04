using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Amphur
{
    public int AmphurId { get; set; }

    public string AmphurCode { get; set; } = null!;

    public string AmphurName { get; set; } = null!;

    public string Postcode { get; set; } = null!;

    public int GeoId { get; set; }

    public int ProvinceId { get; set; }

    public string? AmphurNameEn { get; set; }
}
