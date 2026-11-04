using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbFcmBackup
{
    public Guid? Id { get; set; }

    public string? Token { get; set; }

    public int? UserId { get; set; }

    public int? SchoolId { get; set; }

    public string? Imei { get; set; }

    public string? System { get; set; }

    public string? FdLang { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? FdMbbrand { get; set; }

    public DateTime? FdLoginDate { get; set; }

    public int? FdVerifyStatus { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime? FdFirstLoginDate { get; set; }

    public string? FdLocation { get; set; }
}
