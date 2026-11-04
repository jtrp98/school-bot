using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotLicenseBinding
{
    public int SchoolBotLicenseBindingId { get; set; }

    public int LicenseId { get; set; }

    public string DeviceJkt { get; set; } = null!;

    public string PublicJwk { get; set; } = null!;

    public int Epoch { get; set; }

    public DateTime ActivatedAt { get; set; }

    public DateTime? DetachedAt { get; set; }

    public DateTime LastCheckedAt { get; set; }

    public DateTime LastEntitlementExpiresAt { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
