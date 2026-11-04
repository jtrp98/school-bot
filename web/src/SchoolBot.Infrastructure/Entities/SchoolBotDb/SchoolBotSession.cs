using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotSession
{
    public int SchoolBotSessionId { get; set; }

    public string Channel { get; set; } = null!;

    public int? AccountId { get; set; }

    public string TokenHash { get; set; } = null!;

    public string? ProfileKind { get; set; }

    public int? ProfileId { get; set; }

    public string? LegacyAdminId { get; set; }

    public string? DeviceJkt { get; set; }

    public DateTime? SourceTokenExpiresAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
