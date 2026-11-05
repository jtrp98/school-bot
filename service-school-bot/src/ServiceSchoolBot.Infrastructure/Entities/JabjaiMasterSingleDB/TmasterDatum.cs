using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TmasterDatum
{
    public int MasterId { get; set; }

    public string? MasterCode { get; set; }

    public string? MasterDes { get; set; }

    public string? MasterType { get; set; }

    public bool? IsActive { get; set; }

    public int? MasterOrder { get; set; }

    public string? MasterDesEn { get; set; }
}
