using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TempAddress
{
    public int SEmp { get; set; }

    public int Id { get; set; }

    public int? Type { get; set; }

    public string? No { get; set; }

    public string? VillageNo { get; set; }

    public string? Village { get; set; }

    public string? Building { get; set; }

    public string? Alley { get; set; }

    public string? Road { get; set; }

    public int? SubdistrictId { get; set; }

    public int? DistrictId { get; set; }

    public int? ProvinceId { get; set; }

    public string? Postcode { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool CDel { get; set; }
}
