using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotLicense
{
    public int SchoolBotLicenseId { get; set; }

    public string SchoolName { get; set; } = null!;

    public string KeyHash { get; set; } = null!;

    public string KeyHint { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public int? CurrentBindingId { get; set; }

    public int BindingEpoch { get; set; }

    public string? AllowedNextJkt { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
