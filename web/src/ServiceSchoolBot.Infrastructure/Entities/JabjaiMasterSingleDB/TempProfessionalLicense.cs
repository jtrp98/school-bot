using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TempProfessionalLicense
{
    public int SEmp { get; set; }

    public int Id { get; set; }

    public int? LicenseType { get; set; }

    public string? LicenseNo { get; set; }

    public string? LicenseName { get; set; }

    public DateTime? IssuedDate { get; set; }

    public DateTime? ExpireDate { get; set; }

    public string? AgencyIssued { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool CDel { get; set; }
}
